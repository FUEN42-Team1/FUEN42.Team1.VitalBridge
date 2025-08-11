namespace Team1.VitalBridge.BackStage.Models.DTOs
{
    public class LocationDto
    {
        // 縣市資料
        public record CityDto(int Id, string Name);

        // 鄉鎮資料
        public record TownshipDto(int Id, string Name, string PostalCode);

        // 名稱對應資料
        public record NameResolveDto(string? CityName, string? TownshipName);
    }
}
