namespace Lab3_proje.Models.Entities
{
    public class OgrenciDetay : User // kalıtım
    {
        public string DogumTarihi { get; set; } = string.Empty;
        public string telefonNumarası { get; set; } = string.Empty;
        public string okuduguBolum { get; set; } = string.Empty;

        public int TCKimlik { get; private set; }

        public string belgeTeslimYili { get; set; } = string.Empty;
        public string ikametgahAdresi { get; set; } = string.Empty;

        public string saglikYardimStatüsü { get; set; } = string.Empty;

        public string uyruk { get; set; }

    }
}
