



namespace Lab3_proje.Services.Validation
{
    public abstract class KuralKontrolcu
    {
        public string KuralAdi { get; set; } = string.Empty;

        public abstract bool KontrolEt(object veri, out string hataMesaji);

    }
}


