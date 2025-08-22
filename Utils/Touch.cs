using Alp.Com.Igu.Connections;
using AlpTlc.Biz.AppSettings;
using AlpTlc.Connessione.SettingsFile;
using System;
using System.IO;

namespace Alp.Com.Igu.Utils
{
    public static class AutomationDontTouch
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
                           ("Alp.Com.Igu.Strumenti.AutomationDontTouch");
        
        private static bool IsReady => _pathCompletoFile != null;

        public static string _pathCompletoFile = null;

        public static void InitTouchFile(string pathCompletoFile)
        {
            try
            {
                //string pathCartella = Path.GetDirectoryName(pathCompletoFile);

                //if (!Directory.Exists(pathCartella))
                //{
                //    DirectoryInfo cartella = Directory.CreateDirectory(pathCartella);
                //}

                //if (!File.Exists(pathCompletoFile))
                //{
                //    FileStream myFileStream = File.Open(pathCompletoFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                //    myFileStream.Close();
                //    myFileStream.Dispose();
                //}

                // Sostituito con chiamata a WebApi perché l'interfaccia utente si può trovare in una altro PC rispetto il server
                WebApiRequest.GetInstance().InitTouchFileAsync(pathCompletoFile).GetAwaiter();

                _pathCompletoFile = pathCompletoFile;
            }
            catch (Exception ex)
            {
                _pathCompletoFile = null;
                log.Error( $"Errore in InitTouchFile {pathCompletoFile}: " + ex.Message);
            }
        }

        public static void DoTouch()
        {
            try
            {
                if (IsReady)
                {
                    //// Se il file viene cancellato dopo l'inizializzazione (ad es. con windows explorer), va ripristinato.
                    //if (!File.Exists(_pathCompletoFile))
                    //    InitTouchFile(_pathCompletoFile);
                    //else
                    //    File.SetLastWriteTimeUtc(_pathCompletoFile, DateTime.UtcNow);

                    // Sostituito con chiamata a WebApi perché l'interfaccia utente si può trovare in una altro PC rispetto il server
                    AppSettingsAzioni.GetInstance().DoTouchAsync(_pathCompletoFile).GetAwaiter();

                }
            }
            catch (Exception ex)
            {
                log.Error("Errore in DoTouch: " + ex.Message);
            }

        }
    }
}
