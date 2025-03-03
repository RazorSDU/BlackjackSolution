using Blackjack.MAUI.Models;
using Blackjack.MAUI.PageModels;

namespace Blackjack.MAUI.Pages;

public partial class MainPage : ContentPage
{
	public MainPage(MainPageModel model)
	{
		InitializeComponent();
		BindingContext = model;
	}
}