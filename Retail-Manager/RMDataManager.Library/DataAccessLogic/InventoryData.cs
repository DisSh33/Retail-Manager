﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RMDataManager.Library.DataAccess;
using RMDataManager.Library.Models;

namespace RMDataManager.Library.DataAccessLogic
{
    public class InventoryData : IInventoryData
    {
        private readonly ISqlDataAccess _sql;

        public InventoryData(ISqlDataAccess sqlDataAccess)
        {
            _sql = sqlDataAccess;
        }

        public List<InventoryModel> GetInventory()
        {
            var output = _sql.LoadData<InventoryModel, dynamic>("dbo.spInventory_GetAll", new { }, "RMData");

            return output;
        }

        public void SaveInventoryRecord(InventoryModel item)
        {
            _sql.SaveData("dbo.spInventory_Insert", item, "RMData");
        }
    }
}
