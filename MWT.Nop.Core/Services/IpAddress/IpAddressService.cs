using LinqToDB.Data;
using MWT.Nop.Core.Domain.IpAddress;
using Nop.Data;

namespace Nop.Services.Customizations.IpAddress
{
    public partial class IpAddressService : IIpAddressService
    {
        #region Fields

        private readonly IRepository<IpAddressRecord> _repository;

        #endregion

        #region Ctor

        public IpAddressService(IRepository<IpAddressRecord> repository)
        {
            this._repository = repository;
        }

        #endregion
        public async Task<IpAddressRecord> GetDetailsByIpAddress(string ipAddress)
        {

            return (await _repository.EntityFromSqlAsync("Sp_ReadIPInfo", new DataParameter[] {new DataParameter()
            {
                DataType=LinqToDB.DataType.VarChar,
                Value=ipAddress,
                Direction=System.Data.ParameterDirection.Input,
                Name="@ipAddress",

            } })).FirstOrDefault();
        }

        public async Task Save(IpAddressRecord record)
        {
            await _repository.EntityFromSqlAsync("Sp_InserIPInfo", new DataParameter[] {new DataParameter()
            {
                DataType=LinqToDB.DataType.VarChar,
                Value=record.IpAddress,
                Direction=System.Data.ParameterDirection.Input,
                Name="@ipAddress",

            },
            new DataParameter()
            {
                DataType=LinqToDB.DataType.VarChar,
                Value=record.Country_Code,
                Direction=System.Data.ParameterDirection.Input,
                Name="@country_code",

            },
                        new DataParameter()
            {
                DataType=LinqToDB.DataType.VarChar,
                Value=record.Country,
                Direction=System.Data.ParameterDirection.Input,
                Name="@country",

            },
                        new DataParameter()
            {
                DataType=LinqToDB.DataType.VarChar,
                Value=record.Region,
                Direction=System.Data.ParameterDirection.Input,
                Name="@region",

            },
                        new DataParameter()
            {
                DataType=LinqToDB.DataType.VarChar,
                Value=record.City,
                Direction=System.Data.ParameterDirection.Input,
                Name="@city",

            },
                        new DataParameter()
            {
                DataType=LinqToDB.DataType.VarChar,
                Value=record.Latitude,
                Direction=System.Data.ParameterDirection.Input,
                Name="@latitude",

            },
                        new DataParameter()
            {
                DataType=LinqToDB.DataType.VarChar,
                Value=record.Longitude,
                Direction=System.Data.ParameterDirection.Input,
                Name="@longitude",

            },
                        new DataParameter()
            {
                DataType=LinqToDB.DataType.VarChar,
                Value=record.Zip_Code,
                Direction=System.Data.ParameterDirection.Input,
                Name="@zip_code",

            },
                        new DataParameter()
            {
                DataType=LinqToDB.DataType.VarChar,
                Value=record.Time_Zone,
                Direction=System.Data.ParameterDirection.Input,
                Name="@time_zone",

            }



            });
        }
    }
}
