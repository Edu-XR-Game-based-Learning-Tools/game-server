using Core.Business;
using Cysharp.Threading.Tasks;
using Shared.Extension;
using Shared.Network;

namespace Core.Framework
{
    public class UserDataController : IUserDataController
    {
        public UserServerEntity ServerData { get; private set; }

        private readonly IDefinitionManager _definitionManager;

        public ClassRoomDefinition[] ClassRoomDefinitions { get => _classRoomDefinitions; }
        private ClassRoomDefinition[] _classRoomDefinitions;

        public UserDataController(
            IDefinitionManager definitionManager)
        {
            _definitionManager = definitionManager;
            ServerData = new UserServerEntity();
        }

        public async UniTaskVoid CacheDefinitions()
        {
            var classRoomDefs = await _definitionManager.GetAllDefinition<ClassRoomDefinition>();
            _classRoomDefinitions = classRoomDefs.ToArray();
        }

        public ClassRoomDefinition GetClassRoomDefinition(string id)
        {
            return _classRoomDefinitions.Find(c => c.Id == id);
        }
    }
}
