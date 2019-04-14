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

        
        public t_author_InfoContainer t_author_Infocontainer = new t_author_InfoContainer();
        
        public t_globalContainer t_globalcontainer = new t_globalContainer();
        
        public t_languageContainer t_languagecontainer = new t_languageContainer();
        

		public void loadAll()
		{
			
			t_author_Infocontainer.loadDataFromBin();
			
			t_globalcontainer.loadDataFromBin();
			
			t_languagecontainer.loadDataFromBin();
			
		}
	}
}