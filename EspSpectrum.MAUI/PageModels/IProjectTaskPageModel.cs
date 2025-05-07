using CommunityToolkit.Mvvm.Input;
using EspSpectrum.MAUI.Models;

namespace EspSpectrum.MAUI.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}