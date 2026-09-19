using CommunityToolkit.Mvvm.Input;
using RaizBarApp.Models;

namespace RaizBarApp.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}