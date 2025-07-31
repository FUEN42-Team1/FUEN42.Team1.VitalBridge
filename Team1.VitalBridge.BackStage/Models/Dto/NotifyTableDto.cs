namespace Team1.VitalBridge.BackStage.Models.Dto
{
    public class NotifyTableDto
    {
        public int id { get; set; }
        public string title { get; set; }
        public string notifysUrl { get; set; }
        public string categoriesName { get; set; }
        public string sendDate { get; set; }
        public string validityDate { get; set; }
        public int userCount { get; set; }
    }
}
