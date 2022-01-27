/**
 * Auto generated, do not edit it
 */
using System.Collections.Generic;
using xbuffer;

namespace Data
{
    public class GameDataManager
    {
		public static readonly GameDataManager Singleton = new GameDataManager();

        
        private t_AuthorInfoContainer mt_AuthorInfoContainer = new t_AuthorInfoContainer();
        
        private t_AuthorInfo10Container mt_AuthorInfo10Container = new t_AuthorInfo10Container();
        
        private t_AuthorInfo2Container mt_AuthorInfo2Container = new t_AuthorInfo2Container();
        
        private t_AuthorInfo3Container mt_AuthorInfo3Container = new t_AuthorInfo3Container();
        
        private t_AuthorInfo4Container mt_AuthorInfo4Container = new t_AuthorInfo4Container();
        
        private t_AuthorInfo5Container mt_AuthorInfo5Container = new t_AuthorInfo5Container();
        
        private t_AuthorInfo6Container mt_AuthorInfo6Container = new t_AuthorInfo6Container();
        
        private t_AuthorInfo7Container mt_AuthorInfo7Container = new t_AuthorInfo7Container();
        
        private t_AuthorInfo8Container mt_AuthorInfo8Container = new t_AuthorInfo8Container();
        
        private t_AuthorInfo9Container mt_AuthorInfo9Container = new t_AuthorInfo9Container();
        
        private t_global_bContainer mt_global_bContainer = new t_global_bContainer();
        
        private t_Global10Container mt_Global10Container = new t_Global10Container();
        
        private t_Global2Container mt_Global2Container = new t_Global2Container();
        
        private t_Global3Container mt_Global3Container = new t_Global3Container();
        
        private t_Global4Container mt_Global4Container = new t_Global4Container();
        
        private t_Global5Container mt_Global5Container = new t_Global5Container();
        
        private t_Global6Container mt_Global6Container = new t_Global6Container();
        
        private t_Global7Container mt_Global7Container = new t_Global7Container();
        
        private t_Global8Container mt_Global8Container = new t_Global8Container();
        
        private t_Global9Container mt_Global9Container = new t_Global9Container();
        
        private t_global_iContainer mt_global_iContainer = new t_global_iContainer();
        
        private t_global_sContainer mt_global_sContainer = new t_global_sContainer();
        
        private t_language_cnContainer mt_language_cnContainer = new t_language_cnContainer();
        
        private t_uiContainer mt_uiContainer = new t_uiContainer();
        

		private GameDataManager()
		{
		
		}

		public void loadAll()
		{
			
			mt_AuthorInfoContainer.loadDataFromBin();
			
			mt_AuthorInfo10Container.loadDataFromBin();
			
			mt_AuthorInfo2Container.loadDataFromBin();
			
			mt_AuthorInfo3Container.loadDataFromBin();
			
			mt_AuthorInfo4Container.loadDataFromBin();
			
			mt_AuthorInfo5Container.loadDataFromBin();
			
			mt_AuthorInfo6Container.loadDataFromBin();
			
			mt_AuthorInfo7Container.loadDataFromBin();
			
			mt_AuthorInfo8Container.loadDataFromBin();
			
			mt_AuthorInfo9Container.loadDataFromBin();
			
			mt_global_bContainer.loadDataFromBin();
			
			mt_Global10Container.loadDataFromBin();
			
			mt_Global2Container.loadDataFromBin();
			
			mt_Global3Container.loadDataFromBin();
			
			mt_Global4Container.loadDataFromBin();
			
			mt_Global5Container.loadDataFromBin();
			
			mt_Global6Container.loadDataFromBin();
			
			mt_Global7Container.loadDataFromBin();
			
			mt_Global8Container.loadDataFromBin();
			
			mt_Global9Container.loadDataFromBin();
			
			mt_global_iContainer.loadDataFromBin();
			
			mt_global_sContainer.loadDataFromBin();
			
			mt_language_cnContainer.loadDataFromBin();
			
			mt_uiContainer.loadDataFromBin();
			
		}

		
		public List<t_AuthorInfo> Gett_AuthorInfoList()
		{
			return mt_AuthorInfoContainer.getList();
		}

		public Dictionary<int, t_AuthorInfo> Gett_AuthorInfoMap()
		{
			return mt_AuthorInfoContainer.getMap();
		}
		
		public List<t_AuthorInfo10> Gett_AuthorInfo10List()
		{
			return mt_AuthorInfo10Container.getList();
		}

		public Dictionary<int, t_AuthorInfo10> Gett_AuthorInfo10Map()
		{
			return mt_AuthorInfo10Container.getMap();
		}
		
		public List<t_AuthorInfo2> Gett_AuthorInfo2List()
		{
			return mt_AuthorInfo2Container.getList();
		}

		public Dictionary<int, t_AuthorInfo2> Gett_AuthorInfo2Map()
		{
			return mt_AuthorInfo2Container.getMap();
		}
		
		public List<t_AuthorInfo3> Gett_AuthorInfo3List()
		{
			return mt_AuthorInfo3Container.getList();
		}

		public Dictionary<int, t_AuthorInfo3> Gett_AuthorInfo3Map()
		{
			return mt_AuthorInfo3Container.getMap();
		}
		
		public List<t_AuthorInfo4> Gett_AuthorInfo4List()
		{
			return mt_AuthorInfo4Container.getList();
		}

		public Dictionary<int, t_AuthorInfo4> Gett_AuthorInfo4Map()
		{
			return mt_AuthorInfo4Container.getMap();
		}
		
		public List<t_AuthorInfo5> Gett_AuthorInfo5List()
		{
			return mt_AuthorInfo5Container.getList();
		}

		public Dictionary<int, t_AuthorInfo5> Gett_AuthorInfo5Map()
		{
			return mt_AuthorInfo5Container.getMap();
		}
		
		public List<t_AuthorInfo6> Gett_AuthorInfo6List()
		{
			return mt_AuthorInfo6Container.getList();
		}

		public Dictionary<int, t_AuthorInfo6> Gett_AuthorInfo6Map()
		{
			return mt_AuthorInfo6Container.getMap();
		}
		
		public List<t_AuthorInfo7> Gett_AuthorInfo7List()
		{
			return mt_AuthorInfo7Container.getList();
		}

		public Dictionary<int, t_AuthorInfo7> Gett_AuthorInfo7Map()
		{
			return mt_AuthorInfo7Container.getMap();
		}
		
		public List<t_AuthorInfo8> Gett_AuthorInfo8List()
		{
			return mt_AuthorInfo8Container.getList();
		}

		public Dictionary<int, t_AuthorInfo8> Gett_AuthorInfo8Map()
		{
			return mt_AuthorInfo8Container.getMap();
		}
		
		public List<t_AuthorInfo9> Gett_AuthorInfo9List()
		{
			return mt_AuthorInfo9Container.getList();
		}

		public Dictionary<int, t_AuthorInfo9> Gett_AuthorInfo9Map()
		{
			return mt_AuthorInfo9Container.getMap();
		}
		
		public List<t_global_b> Gett_global_bList()
		{
			return mt_global_bContainer.getList();
		}

		public Dictionary<string, t_global_b> Gett_global_bMap()
		{
			return mt_global_bContainer.getMap();
		}
		
		public List<t_Global10> Gett_Global10List()
		{
			return mt_Global10Container.getList();
		}

		public Dictionary<int, t_Global10> Gett_Global10Map()
		{
			return mt_Global10Container.getMap();
		}
		
		public List<t_Global2> Gett_Global2List()
		{
			return mt_Global2Container.getList();
		}

		public Dictionary<int, t_Global2> Gett_Global2Map()
		{
			return mt_Global2Container.getMap();
		}
		
		public List<t_Global3> Gett_Global3List()
		{
			return mt_Global3Container.getList();
		}

		public Dictionary<int, t_Global3> Gett_Global3Map()
		{
			return mt_Global3Container.getMap();
		}
		
		public List<t_Global4> Gett_Global4List()
		{
			return mt_Global4Container.getList();
		}

		public Dictionary<int, t_Global4> Gett_Global4Map()
		{
			return mt_Global4Container.getMap();
		}
		
		public List<t_Global5> Gett_Global5List()
		{
			return mt_Global5Container.getList();
		}

		public Dictionary<int, t_Global5> Gett_Global5Map()
		{
			return mt_Global5Container.getMap();
		}
		
		public List<t_Global6> Gett_Global6List()
		{
			return mt_Global6Container.getList();
		}

		public Dictionary<int, t_Global6> Gett_Global6Map()
		{
			return mt_Global6Container.getMap();
		}
		
		public List<t_Global7> Gett_Global7List()
		{
			return mt_Global7Container.getList();
		}

		public Dictionary<int, t_Global7> Gett_Global7Map()
		{
			return mt_Global7Container.getMap();
		}
		
		public List<t_Global8> Gett_Global8List()
		{
			return mt_Global8Container.getList();
		}

		public Dictionary<int, t_Global8> Gett_Global8Map()
		{
			return mt_Global8Container.getMap();
		}
		
		public List<t_Global9> Gett_Global9List()
		{
			return mt_Global9Container.getList();
		}

		public Dictionary<int, t_Global9> Gett_Global9Map()
		{
			return mt_Global9Container.getMap();
		}
		
		public List<t_global_i> Gett_global_iList()
		{
			return mt_global_iContainer.getList();
		}

		public Dictionary<string, t_global_i> Gett_global_iMap()
		{
			return mt_global_iContainer.getMap();
		}
		
		public List<t_global_s> Gett_global_sList()
		{
			return mt_global_sContainer.getList();
		}

		public Dictionary<string, t_global_s> Gett_global_sMap()
		{
			return mt_global_sContainer.getMap();
		}
		
		public List<t_language_cn> Gett_language_cnList()
		{
			return mt_language_cnContainer.getList();
		}

		public Dictionary<string, t_language_cn> Gett_language_cnMap()
		{
			return mt_language_cnContainer.getMap();
		}
		
		public List<t_ui> Gett_uiList()
		{
			return mt_uiContainer.getList();
		}

		public Dictionary<string, t_ui> Gett_uiMap()
		{
			return mt_uiContainer.getMap();
		}
		
	}
}