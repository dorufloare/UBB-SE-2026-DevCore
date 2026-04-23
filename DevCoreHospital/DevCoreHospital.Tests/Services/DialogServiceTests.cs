using System.Threading.Tasks;
using DevCoreHospital.Services;

namespace DevCoreHospital.Tests.Services
{
    public class DialogServiceTests
    {
        private readonly DialogService service;

        public DialogServiceTests()
        {
            service = new DialogService();
        }

        [Fact]
        public async Task ShowMessageAsync_CompletesWithoutThrowing_WhenXamlRootIsNotSet()
        {
            await service.ShowMessageAsync("Title", "Message");
        }

        [Fact]
        public async Task ShowMessageAsync_CompletesWithoutThrowing_WhenTitleIsEmpty()
        {
            await service.ShowMessageAsync(string.Empty, "Message");
        }

        [Fact]
        public async Task ShowMessageAsync_CompletesWithoutThrowing_WhenMessageIsEmpty()
        {
            await service.ShowMessageAsync("Title", string.Empty);
        }

        [Fact]
        public async Task ShowMessageAsync_CompletesWithoutThrowing_WhenBothStringsAreEmpty()
        {
            await service.ShowMessageAsync(string.Empty, string.Empty);
        }
    }
}
