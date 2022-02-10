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

        #CONTAINER_MEMBER_LOOP#
        private #CLASS_NAME#Container #CLASS_NAME#Container = new #CLASS_NAME#Container();
        #CONTAINER_MEMBER_LOOP#

		private GameDataManager()
		{
		
		}

		public void loadAll()
		{
			#CONTAINER_LOAD_LOOP#
			#LOOP_CLASS_NAME#Container.loadDataFromBin();
			#CONTAINER_LOAD_LOOP#
		}

		#CONTAINER_GET_LOOP#
		public List<#LOOP_CLASS_NAME#> Get#LOOP_CLASS_NAME#List()
		{
			return #LOOP_CLASS_NAME#Container.getList();
		}

		public Dictionary<#ID_TYPE#, #LOOP_CLASS_NAME#> Get#LOOP_CLASS_NAME#Map()
		{
			return #LOOP_CLASS_NAME#Container.getMap();
		}
		#CONTAINER_GET_LOOP#
	}
}