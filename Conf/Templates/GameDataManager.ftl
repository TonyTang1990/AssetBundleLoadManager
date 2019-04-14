/**
 * Auto generated, do not edit it
 */
using xbuffer;

namespace Data
{
    /// <summary>
    /// 数据加载管理类
    /// </summary>
    public class GameDataManager : SingletonTemplate<GameDataManager>
    {
        public GameDataManager()
        {

        }

        #CONTAINER_MEMBER_LOOP#
        public #CLASS_NAME#Container #CLASS_NAME#container = new #CLASS_NAME#Container();
        #CONTAINER_MEMBER_LOOP#

		public void loadAll()
		{
			#CONTAINER_LOAD_LOOP#
			#LOOP_CLASS_NAME#container.loadDataFromBin();
			#CONTAINER_LOAD_LOOP#
		}
	}
}