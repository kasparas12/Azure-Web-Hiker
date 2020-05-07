using Azure.Web.Hiker.Core.AgentRegistrar.Persistence;
using Azure.Web.Hiker.Core.AgentRegistrar.Services;
using Azure.Web.Hiker.Core.Common.Settings;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace Azure.Web.Hiker.Tests.Core
{
    [TestClass]
    public class AgentRegistrarServiceTests
    {
        private Mock<IAgentRegistrarRepository> _repositoryMock;
        private Mock<IGeneralApplicationSettings> _generalApplicationSettingsMock;
        private AgentRegistrarService _service;

        public AgentRegistrarServiceTests()
        {
            _repositoryMock = new Mock<IAgentRegistrarRepository>();
            _generalApplicationSettingsMock = new Mock<IGeneralApplicationSettings>();
            _service = new AgentRegistrarService(_repositoryMock.Object, _generalApplicationSettingsMock.Object);
        }

        [TestMethod]
        public void AgentForSpecificHostExists_WhenAgentNotFound_ShouldReturnFalse()
        {
            //Arrange
            _repositoryMock.Setup(x => x.AgentForSpecificHostExists(It.IsAny<string>())).Returns(false);

            //Act
            var agentExists = _service.AgentExistsForGivenHostName("dummy");

            //Assert
            Assert.IsFalse(agentExists);
        }

        [TestMethod]
        public void AgentForSpecificHostExists_WhenAgentFound_ShouldReturnTrue()
        {
            //Arrange
            _repositoryMock.Setup(x => x.AgentForSpecificHostExists(It.IsAny<string>())).Returns(true);

            //Act
            var agentExists = _service.AgentExistsForGivenHostName("dummy");

            //Assert
            Assert.IsTrue(agentExists);
        }
    }
}
