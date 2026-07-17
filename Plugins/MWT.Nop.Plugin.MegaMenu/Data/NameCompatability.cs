using Nop.Data.Mapping;
using MWT.Nop.Plugin.MegaMenu.Domain;
using System;
using System.Collections.Generic;

namespace MWT.Nop.Plugin.MegaMenu.Data
{
    public class NameCompatability : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>()
    {
      {
        typeof (Menu),
        "MWT_MM_Menu"
      },
      {
        typeof (MenuItem),
        "MWT_MM_MenuItem"
      },
       {
        typeof (EntityWidgetMapping),
        "MWT_MAP_EntityWidgetMapping"
      },{
        typeof (EntityMapping),
        "MWT_MAP_EntityMapping"
      },
    };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>();
    }
}