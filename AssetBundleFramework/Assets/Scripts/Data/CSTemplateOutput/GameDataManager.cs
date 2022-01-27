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

        
        private t_author_InfoContainer mt_author_InfoContainer = new t_author_InfoContainer();
        
        private t_global_bContainer mt_global_bContainer = new t_global_bContainer();
        
        private t_global_iContainer mt_global_iContainer = new t_global_iContainer();
        
        private t_global_sContainer mt_global_sContainer = new t_global_sContainer();
        
        private t_language_cnContainer mt_language_cnContainer = new t_language_cnContainer();
        

		private GameDataManager()
		{
		
		}

		public void loadAll()
		{
			
			mt_author_InfoContainer.loadDataFromBin();
			
			mt_global_bContainer.loadDataFromBin();
			
			mt_global_iContainer.loadDataFromBin();
			
			mt_global_sContainer.loadDataFromBin();
			
			mt_language_cnContainer.loadDataFromBin();
			
		}

		
		public List<t_author_Info> Gett_author_InfoList()
		{
			return mt_author_InfoContainer.getList();
		}

		public Dictionary<int, t_author_Info> Gett_author_InfoMap()
		{
			return mt_author_InfoContainer.getMap();
		}
		
		public List<t_global_b> Gett_global_bList()
		{
			return mt_global_bContainer.getList();
		}

		public Dictionary<string, t_global_b> Gett_global_bMap()
		{
			return mt_global_bContainer.getMap();
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
		
	}
}