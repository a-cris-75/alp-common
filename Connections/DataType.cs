using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Alp.Com.Igu
{

    public class ApplicationSettingsStatic
    {

        public static string? PercorsoFileImpostazioniGenerali { get; set; }

        public static string? PercorsoDumpImmagineIngresso { get; set; }

        public static string? PercorsoDumpImmagineUscita { get; set; }

        public static string? PercorsoFileImpostazioniDia { get; set; }

        public static int FrameRateLive { get; set; }

        public static bool ImpostazioniAbilitate { get; set; }

        public static string? AutomationDontTouchFile { get; set; }

        public static string ServerIP { get; set; } = "localhost";

        public static ushort RitardoOnOffIlluminatoriMs { get; set; }

        public static LimitiImpostazioni? LimitiImpostazioni { get; set; }

    }

    public class LimitiImpostazioni
    {
        public decimal EsposizioneMsMin { get; set; }

        public decimal EsposizioneMsMax { get; set; }

        public int EsposizioneMsNTaccheMin { get; set; }

        public decimal GuadagnoMin { get; set; }

        public decimal GuadagnoMax { get; set; }

        public int GuadagnoNTaccheMin { get; set; }

        public decimal FreqAcquisizioneImmaginiMin { get; set; }

        public decimal FreqAcquisizioneImmaginiMax { get; set; }

        public int FreqAcquisizioneImmaginiNTaccheMin { get; set; }

    }

    public class ImpostazioneGenerale 
    {
        [Key, Column(Order = 0)]
        [StringLength(50)]
        public string? Id { get; set; }

        private string? valore;

        [StringLength(255)]
        public string? Valore
        {
            get { return valore; }
            set
            {
                if (valore == null || !valore.Equals(value))
                    isModified = true;

                valore = value;
            }
        }

        private bool isModified = false;
        public bool IsModified 
        { 
            get {return isModified; } 
        }

        public bool Persistente { get; set; }

        //public ImpostazioneGenerale()
        //{
        //    Id = string.Empty;
        //    Valore = string.Empty;
        //}

    }

}
