using Prism.Mvvm;

namespace MyLrcMaker.Infrastructure
{
    public interface IIOService
    {
        void ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : BindableBase;
    }
}