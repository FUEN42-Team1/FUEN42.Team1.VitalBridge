using Team1.VitalBridge.BackStage.Models.ViewModels;

namespace Team1.VitalBridge.BackStage.Models.Interfaces
{
	public interface IProductViewModel
	{
		List<int> SelectedCategoryIds { get; set; }
		List<CategorySelectionItemViewModel> Categories { get; set; }
		List<int> SelectedShipIds { get; set; }
		List<ShipSelectionItemViewModel> Ships { get; set; }
	}
}
