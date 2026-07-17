//using AutoMapper;
//using AutoMapper.Configuration;
//using System.Threading;


//namespace MWT.Nop.Plugin.MegaMenu.AutoMapper
//{
//    public static class AutoMapperConfigurationMwt
//    {
//        private static MapperConfigurationExpression _mapperConfigurationExpression;
//        private static IMapper _mapper;
//        private static readonly object mapperConfigurationExpressionLockObject = new object();
//        private static readonly object mapperLockObject = new object();

//        public static MapperConfigurationExpression MapperConfigurationExpression
//        {
//            get
//            {
//                if (AutoMapperConfigurationMwt._mapperConfigurationExpression == null)
//                {
//                    object expressionLockObject = AutoMapperConfigurationMwt.mapperConfigurationExpressionLockObject;
//                    bool lockTaken = false;
//                    try
//                    {
//                        Monitor.Enter(expressionLockObject, ref lockTaken);
//                        if (AutoMapperConfigurationMwt._mapperConfigurationExpression == null)
//                            AutoMapperConfigurationMwt._mapperConfigurationExpression = new MapperConfigurationExpression();
//                    }
//                    finally
//                    {
//                        if (lockTaken)
//                            Monitor.Exit(expressionLockObject);
//                    }
//                }
//                return AutoMapperConfigurationMwt._mapperConfigurationExpression;
//            }
//        }

//        public static IMapper Mapper
//        {
//            get
//            {
//                if (AutoMapperConfigurationMwt._mapper == null)
//                {
//                    object mapperLockObject = AutoMapperConfigurationMwt.mapperLockObject;
//                    bool lockTaken = false;
//                    try
//                    {
//                        Monitor.Enter(mapperLockObject, ref lockTaken);
//                        if (AutoMapperConfigurationMwt._mapper == null)
//                            AutoMapperConfigurationMwt._mapper = new MapperConfiguration(AutoMapperConfigurationMwt.MapperConfigurationExpression).CreateMapper();
//                    }
//                    finally
//                    {
//                        if (lockTaken)
//                            Monitor.Exit(mapperLockObject);
//                    }
//                }
//                return AutoMapperConfigurationMwt._mapper;
//            }
//        }

//        public static TDestination MapTo<TSource, TDestination>(this TSource source) => ((IMapperBase)AutoMapperConfigurationMwt.Mapper).Map<TSource, TDestination>(source);

//        public static TDestination MapTo<TSource, TDestination>(
//          this TSource source,
//          TDestination destination)
//        {
//            return ((IMapperBase)AutoMapperConfigurationMwt.Mapper).Map<TSource, TDestination>(source, destination);
//        }
//    }
//}
