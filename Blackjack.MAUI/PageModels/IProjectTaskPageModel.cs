using CommunityToolkit.Mvvm.Input;
using Blackjack.MAUI.Models;

namespace Blackjack.MAUI.PageModels;

public interface IProjectTaskPageModel
{
	IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
	bool IsBusy { get; }
}