using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AlpTlc.Connessione;
using AlpTlc.Biz.Core;
using AlpTlc.Connessione.WebAPI.RemotaInOut;
using System.Threading;
using AlpTlc.Domain.StatoMacchina;

namespace Alp.Com.Igu.Core
{
    public class RemotaInOutAzioni
    {

        Serilog.ILogger _logger = Serilog.Log.ForContext(typeof(RemotaInOutAzioni));

        private static readonly RemotaInOutAzioni _remotaInOutAzioni = new RemotaInOutAzioni();

        private static readonly RemotaInOutAzioniBaseTelecamere _remotaInOutAzioniBaseTelecamere = RemotaInOutAzioniBaseTelecamere.GetInstance();
        private static readonly RemotaInOutAzioniBaseIlluminatori _remotaInOutAzioniBaseIlluminatori = RemotaInOutAzioniBaseIlluminatori.GetInstance();

        static ushort RITARDO_RESET_TELECAMERE_MS;
        static ushort RITARDO_ACCENSIONE_ILLUMINATORI_MS;

        bool lockAzioniIlluminatori = false;
        bool lockAzioniTlc = false;

        static RemotaInOutAzioni()
        {
        }

        private RemotaInOutAzioni()
        {
            _logger.Verbose("RemotaInOutAzioni ctor-");

            // TODO da impostazioni applicazione
            RITARDO_RESET_TELECAMERE_MS = 5000;
            RITARDO_ACCENSIONE_ILLUMINATORI_MS = 2500;
        }

        public static RemotaInOutAzioni GetInstance() => _remotaInOutAzioni;

        public async Task<Esito> VerificaEAccendiTelecamereAsync(bool checkLock = false)
        {
            _logger.Verbose($"VerificaEAccendiTelecamereAsync... checkLock: [{checkLock}]");

            if (checkLock && lockAzioniTlc)
                return new Esito { Eccezione = null, Titolo = "Attenzione", Messaggio = "Attendere il termine dell'azione precedente sulle telecamere prima di effettuarne un'altra.", Ok = false, Icona = System.Drawing.SystemIcons.Warning };

            if (checkLock) lockAzioniTlc = true;

            Esito esito = new Esito { Eccezione = null, Titolo = "Informazione", Messaggio = "Accensione telecamere andata a buon fine", Ok = true, Icona = System.Drawing.SystemIcons.Information };

            try
            {
                if (await _remotaInOutAzioniBaseTelecamere.IsTlc1Off())
                {
                    _logger.Information($"VerificaEAccendiTelecamereAsync: la telecamera 1 è spenta. Accendiamola...");
                    _ = await _remotaInOutAzioniBaseTelecamere.SwitchTlc1On();
                }

                if (await _remotaInOutAzioniBaseTelecamere.IsTlc2Off())
                {
                    _logger.Information($"VerificaEAccendiTelecamereAsync: la telecamera 2 è spenta. Accendiamola...");
                    _ = await _remotaInOutAzioniBaseTelecamere.SwitchTlc2On();
                }
            }
            catch (Exception ex)
            {
                esito = new Esito { Eccezione = ex, Titolo = "Errore accensione telecamere", Messaggio = ex.Message, Ok = false, Icona = System.Drawing.SystemIcons.Error };
            }

            if (checkLock) lockAzioniTlc = false;

            _logger.Verbose($"VerificaEAccendiTelecamereAsync... esito: [{esito}]");

            return esito;
        }

        public async Task<Esito> SpegniTelecamereAsync(bool checkLock = false)
        {
            _logger.Verbose($"SpegniTelecamereAsync... checkLock: [{checkLock}]");

            if (checkLock && lockAzioniTlc)
                return new Esito { Eccezione = null, Titolo = "Attenzione", Messaggio = "Attendere il termine dell'azione precedente sulle telecamere prima di effettuarne un'altra.", Ok = false, Icona = System.Drawing.SystemIcons.Warning };

            if (checkLock) lockAzioniTlc = true;

            Esito esito = new Esito { Eccezione = null, Titolo = "Informazione", Messaggio = "Spegnimento telecamere andato a buon fine", Ok = true, Icona = System.Drawing.SystemIcons.Information };

            try
            {
                if (await _remotaInOutAzioniBaseTelecamere.IsTlc1On())
                    _ = await _remotaInOutAzioniBaseTelecamere.SwitchTlc1Off();

                if (await _remotaInOutAzioniBaseTelecamere.IsTlc2On())
                    _ = await _remotaInOutAzioniBaseTelecamere.SwitchTlc2Off();
            }
            catch (Exception ex)
            {
                esito = new Esito { Eccezione = ex, Titolo = "Errore spegnimento telecamere", Messaggio = ex.Message, Ok = false, Icona = System.Drawing.SystemIcons.Error };
            }

            if (checkLock) lockAzioniTlc = false;

            _logger.Verbose($"SpegniTelecamereAsync... esito: [{esito}]");

            return esito;
        }

        public async Task<Esito> ResetTelecamereAsync(bool checkLock = false)
        {
            _logger.Verbose($"ResetTelecamereAsync... checkLock: [{checkLock}]");

            if (checkLock && lockAzioniTlc)
                return new Esito { Eccezione = null, Titolo = "Attenzione", Messaggio = "Attendere il termine dell'azione precedente sulle telecamere prima di effettuarne un'altra.", Ok = false, Icona = System.Drawing.SystemIcons.Warning };

            if (checkLock) lockAzioniTlc = true;

            Esito esito = new Esito { Eccezione = null, Titolo = "Informazione", Messaggio = "Reset telecamere andato a buon fine", Ok = true, Icona = System.Drawing.SystemIcons.Information };

            try
            {
                esito = await SpegniTelecamereAsync();
                if (esito.Ok)
                {
                    await Task.Delay(RITARDO_RESET_TELECAMERE_MS);
                    esito = await VerificaEAccendiTelecamereAsync();
                }
            }
            catch (Exception ex)
            {
                esito = new Esito { Eccezione = ex, Titolo = "Errore reset telecamere", Messaggio = ex.Message, Ok = false, Icona = System.Drawing.SystemIcons.Error };
            }

            if (checkLock) lockAzioniTlc = false;

            _logger.Verbose($"ResetTelecamereAsync... esito: [{esito}]");

            return esito;
        }

        public async Task<Esito> AccendiIlluminatoriAsync(bool checkLock = false)
        {
            _logger.Verbose($"AccendiIlluminatoriAsync... checkLock: [{checkLock}]");

            if (checkLock && lockAzioniIlluminatori)
                return new Esito { Eccezione = null, Titolo = "Attenzione", Messaggio = "Attendere il termine dell'azione precedente sugli illuminatori prima di effettuarne un'altra.", Ok = false, Icona = System.Drawing.SystemIcons.Warning };

            if (checkLock) lockAzioniIlluminatori = true;

            Esito esito = new Esito { Eccezione = null, Titolo = "Informazione", Messaggio = "Accensione illuminatori andata a buon fine", Ok = true, Icona = System.Drawing.SystemIcons.Information };

            try
            {
                if (await _remotaInOutAzioniBaseIlluminatori.AreIlluminatoriOff())
                {
                    _ = await _remotaInOutAzioniBaseIlluminatori.SwitchIlluminatoriOn();
                }
            }
            catch (Exception ex)
            {
                esito = new Esito { Eccezione = ex, Titolo = "Errore accensione illuminatori", Messaggio = ex.Message, Ok = false, Icona = System.Drawing.SystemIcons.Error };
            }

            if (checkLock) lockAzioniIlluminatori = false;

            _logger.Verbose($"AccendiIlluminatoriAsync... esito: [{esito}]");

            return esito;
        }

        public async Task<Esito> SpegniIlluminatoriAsync(bool checkLock = false)
        {

            _logger.Verbose($"SpegniIlluminatoriAsync... checkLock: [{checkLock}]");

            if (checkLock && lockAzioniIlluminatori)
                return new Esito { Eccezione = null, Titolo = "Attenzione", Messaggio = "Attendere il termine dell'azione precedente sugli illuminatori prima di effettuarne un'altra.", Ok = false, Icona = System.Drawing.SystemIcons.Warning };

            if (checkLock) lockAzioniIlluminatori = true;

            Esito esito = new Esito { Eccezione = null, Titolo = "Informazione", Messaggio = "Spegnimento illuminatori andato a buon fine", Ok = true, Icona = System.Drawing.SystemIcons.Information };

            try
            {
                if (await _remotaInOutAzioniBaseIlluminatori.AreIlluminatoriOn())
                {
                    _ = await _remotaInOutAzioniBaseIlluminatori.SwitchIlluminatoriOff();
                }
            }
            catch (Exception ex)
            {
                esito = new Esito { Eccezione = ex, Titolo = "Errore spegnimento illuminatori", Messaggio = ex.Message, Ok = false, Icona = System.Drawing.SystemIcons.Error };
            }

            if (checkLock) lockAzioniIlluminatori = false;

            _logger.Verbose($"SpegniIlluminatoriAsync... esito: [{esito}]");

            return esito;
        }

        public async Task<Esito> VerificaEAccendiIlluminatoriETelecamereAsync(bool checkLock = false, bool forzaRitardoAccensioneTlc = false)
        {
            _logger.Verbose($"VerificaEAccendiIlluminatoriETelecamereAsync... checkLock: [{checkLock}], forzaRitardoAccensioneTlc: [{forzaRitardoAccensioneTlc}]");

            if (checkLock && (lockAzioniIlluminatori || lockAzioniTlc))
                return new Esito { Eccezione = null, Titolo = "Attenzione", Messaggio = "Attendere il termine dell'azione precedente su illuminatori o telecamere prima di effettuarne un'altra.", Ok = false, Icona = System.Drawing.SystemIcons.Warning };

            if (checkLock)
            {
                lockAzioniIlluminatori = true;
                lockAzioniTlc = true;
            }

            Esito esito = new Esito { Eccezione = null, Titolo = "Informazione", Messaggio = "Verifica ed eventuale accensione di illuminatori e telecamere andata a buon fine", Ok = true, Icona = System.Drawing.SystemIcons.Information };

            try
            {
                if (await _remotaInOutAzioniBaseIlluminatori.AreIlluminatoriOff())
                {
                    _ = await AccendiIlluminatoriAsync();

                    esito = new Esito { Eccezione = null, Titolo = "Informazione", Messaggio = "Accensione illuminatori effettuata!", Ok = true, Icona = System.Drawing.SystemIcons.Exclamation };

                    // Usiamo lo stesso ritardo utilizzato tra i due illuminatori
                    if (!forzaRitardoAccensioneTlc) await Task.Delay(RITARDO_ACCENSIONE_ILLUMINATORI_MS);
                }

                if (await _remotaInOutAzioniBaseTelecamere.IsTlc1Off() || await _remotaInOutAzioniBaseTelecamere.IsTlc2Off())
                {
                    // Il ritardo viene spostato qui (cioè viene effettuato anche a illuminatori accesi) alla verifica temporizzata,
                    // perché se c'è stato un segnale di Reset Telecamere da interfaccia  utente,
                    // non vogliamo che il timer le riaccenda subito, deve comunue attendere.
                    if (forzaRitardoAccensioneTlc) await Task.Delay(RITARDO_ACCENSIONE_ILLUMINATORI_MS);
                    _ = await VerificaEAccendiTelecamereAsync();

                    if (esito.Icona == System.Drawing.SystemIcons.Exclamation)
                        esito = new Esito { Eccezione = null, Titolo = "Informazione", Messaggio = "Accensione illuminatori e telecamere effettuata!", Ok = true, Icona = System.Drawing.SystemIcons.Exclamation };
                    else
                        esito = new Esito { Eccezione = null, Titolo = "Informazione", Messaggio = "Accensione telecamere effettuata!", Ok = true, Icona = System.Drawing.SystemIcons.Exclamation };

                }
            }
            catch (Exception ex)
            {
                esito = new Esito { Eccezione = ex, Titolo = "Errore verifica ed eventuale accensione illuminatori e telecamere", Messaggio = ex.Message, Ok = false, Icona = System.Drawing.SystemIcons.Error };
            }

            if (checkLock)
            {
                lockAzioniIlluminatori = false;
                lockAzioniTlc = false;
            }

            _logger.Verbose($"VerificaEAccendiIlluminatoriETelecamereAsync... esito: [{esito}]");

            return esito;
        }

        public async Task<Esito> VerificaESpegniIlluminatoriETelecamereAsync(bool checkLock = false)
        {
            _logger.Verbose($"VerificaESpegniIlluminatoriETelecamereAsync... checkLock: [{checkLock}]");

            if (checkLock && (lockAzioniIlluminatori || lockAzioniTlc))
                return new Esito { Eccezione = null, Titolo = "Attenzione", Messaggio = "Attendere il termine dell'azione precedente su illuminatori o telecamere prima di effettuarne un'altra.", Ok = false, Icona = System.Drawing.SystemIcons.Warning };

            if (checkLock)
            {
                lockAzioniIlluminatori = true;
                lockAzioniTlc = true;
            }

            Esito esito = new Esito { Eccezione = null, Titolo = "Informazione", Messaggio = "Spegnimento di illuminatori e telecamere andata a buon fine", Ok = true, Icona = System.Drawing.SystemIcons.Information };

            try
            {
                if (await _remotaInOutAzioniBaseIlluminatori.AreIlluminatoriOn())
                {
                    _ = await SpegniIlluminatoriAsync();

                    esito = new Esito { Eccezione = null, Titolo = "Informazione", Messaggio = "Spegnimento illuminatori effettuato!", Ok = true, Icona = System.Drawing.SystemIcons.Exclamation };

                    // Usiamo lo stesso ritardo utilizzato tre i due illuminatori (.. per lo spegnimento serve..?)
                    await Task.Delay(RITARDO_ACCENSIONE_ILLUMINATORI_MS);
                }

                if (await _remotaInOutAzioniBaseTelecamere.IsTlc1On() || await _remotaInOutAzioniBaseTelecamere.IsTlc2On())
                {
                    _ = await SpegniTelecamereAsync();
                    if (esito.Icona == System.Drawing.SystemIcons.Exclamation)
                        esito = new Esito { Eccezione = null, Titolo = "Informazione", Messaggio = "Spegnimento illuminatori e telecamere effettuato!", Ok = true, Icona = System.Drawing.SystemIcons.Exclamation };
                    else
                        esito = new Esito { Eccezione = null, Titolo = "Informazione", Messaggio = "Spegnimento telecamere effettuato!", Ok = true, Icona = System.Drawing.SystemIcons.Exclamation };

                }
            }
            catch (Exception ex)
            {
                esito = new Esito { Eccezione = ex, Titolo = "Errore verifica e spegnimento illuminatori e telecamere", Messaggio = ex.Message, Ok = false, Icona = System.Drawing.SystemIcons.Error };
            }

            if (checkLock)
            {
                lockAzioniIlluminatori = false;
                lockAzioniTlc = false;
            }

            _logger.Verbose($"VerificaESpegniIlluminatoriETelecamereAsync... esito: [{esito}]");

            return esito;
        }

        public async Task<Esito> VerificaStatoIlluminatoriAsync()
        {
            //_logger.Verbose($"VerificaStatoIlluminatoriAsync..."); // RIESUMARE..?

            Esito esito = new Esito { Eccezione = null, Titolo = "Informazione", Messaggio = "Gli illuminatori sono accesi", Ok = true, Icona = System.Drawing.SystemIcons.Information };

            try
            {

                Task<bool> task1 = _remotaInOutAzioniBaseIlluminatori.AreIlluminatoriOff();

                await Task.WhenAll(task1);

                bool areIlluminatoriOff = task1.Result;

                if (areIlluminatoriOff)
                {
                    esito = new Esito { Eccezione = null, Titolo = "Informazione", Messaggio = "Gli illuminatori sono spenti", Ok = true, Icona = System.Drawing.SystemIcons.Exclamation };
                }

            }
            catch (Exception ex)
            {
                esito = new Esito { Eccezione = ex, Titolo = "Errore verifica accensione illuminatori", Messaggio = ex.Message, Ok = false, Icona = System.Drawing.SystemIcons.Error };
            }

            //_logger.Verbose($"VerificaStatoIlluminatoriAsync... esito: [{esito}]");// RIESUMARE..?

            return esito;
        }

        public async Task<Esito> VerificaEAggiornaStatoIlluminatoriAsync()
        {
            //_logger.Verbose($"VerificaEAggiornaStatoIlluminatoriAsync..."); // RIESUMARE..?

            Esito esito = new Esito { Eccezione = null, Titolo = "Informazione", Messaggio = "Gli illuminatori sono accesi", Ok = true, Icona = System.Drawing.SystemIcons.Information };

            try
            {

                Task<bool> task1 = _remotaInOutAzioniBaseIlluminatori.AreIlluminatoriOn();

                await Task.WhenAll(task1);

                StatoRemota.AreIlluminatoriOn = task1.Result;

                if (!StatoRemota.AreIlluminatoriOn)
                {
                    esito = new Esito { Eccezione = null, Titolo = "Informazione", Messaggio = "Gli illuminatori sono spenti", Ok = true, Icona = System.Drawing.SystemIcons.Exclamation };
                }

                StatoRemota.IsRemotaAlive = true;
            }
            catch (Exception ex)
            {
                StatoRemota.IsRemotaAlive = false;
                esito = new Esito { Eccezione = ex, Titolo = "Errore verifica accensione illuminatori", Messaggio = ex.Message, Ok = false, Icona = System.Drawing.SystemIcons.Error };
            }

            //_logger.Verbose($"VerificaEAggiornaStatoIlluminatoriAsync... esito: [{esito}]");// RIESUMARE..?

            return esito;
        }

        public async Task<Esito> VerificaEAggiornaStatoTelecamereAsync()
        {
            //_logger.Verbose($"VerificaEAggiornaStatoTelecamereAsync..."); // RIESUMARE..?

            Esito esito = new Esito { Eccezione = null, Titolo = "Informazione", Messaggio = "Le telecamere sono accese", Ok = true, Icona = System.Drawing.SystemIcons.Information };

            try
            {

                Task<bool> task1 = _remotaInOutAzioniBaseTelecamere.IsTlc1On();
                Task<bool> task2 = _remotaInOutAzioniBaseTelecamere.IsTlc2On();

                await Task.WhenAll(task1, task2);

                StatoRemota.IsTelecamera1On = task1.Result;
                StatoRemota.IsTelecamera2On = task2.Result;

                if (!StatoRemota.IsTelecamera1On && !StatoRemota.IsTelecamera2On)
                {
                    esito = new Esito { Eccezione = null, Titolo = "Informazione", Messaggio = "Le telecamere sono spente", Ok = true, Icona = System.Drawing.SystemIcons.Exclamation };
                }
                else if (!StatoRemota.IsTelecamera1On)
                {
                    esito = new Esito { Eccezione = null, Titolo = "Informazione", Messaggio = "La telecamera n. 1 è spenta", Ok = true, Icona = System.Drawing.SystemIcons.Exclamation };
                }
                else if (!StatoRemota.IsTelecamera1On)
                {
                    esito = new Esito { Eccezione = null, Titolo = "Informazione", Messaggio = "La telecamera n. 2 è spenta", Ok = true, Icona = System.Drawing.SystemIcons.Exclamation };
                }

                StatoRemota.IsRemotaAlive = true;
            }
            catch (Exception ex)
            {
                StatoRemota.IsRemotaAlive = false;
                esito = new Esito { Eccezione = ex, Titolo = "Errore verifica accensione telecamere", Messaggio = ex.Message, Ok = false, Icona = System.Drawing.SystemIcons.Error };
            }

            //_logger.Verbose($"VerificaEAggiornaStatoTelecamereAsync... esito: [{esito}]");// RIESUMARE..?

            return esito;
        }

    }

}
