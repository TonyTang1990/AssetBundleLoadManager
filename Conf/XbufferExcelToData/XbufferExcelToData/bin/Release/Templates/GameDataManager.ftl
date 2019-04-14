/**
 * Auto generated, do not edit it
 */
using xbuffer;

namespace Data
{
    public class GameDataManager
    {
		public static readonly GameDataManager Instance = new GameDataManager();

        #CONTAINER_MEMBER_LOOP#
        public #CLASS_NAME#Container #CLASS_NAME#container = new #CLASS_NAME#Container();
        #CONTAINER_MEMBER_LOOP#

		private GameDataManager()
		{
		
		}

		public void loadAll()
		{
			#CONTAINER_LOAD_LOOP#
			#LOOP_CLASS_NAME#container.loadDataFromBin();
			#CONTAINER_LOAD_LOOP#
		}
	}
}