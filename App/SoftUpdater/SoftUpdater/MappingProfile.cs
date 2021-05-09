///Copyright 2021 Dmitriy Rokoth
///Licensed under the Apache License, Version 2.0
///
///ref 1

using AutoMapper;

namespace SoftUpdater.SoftUpdaterHost
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Db.Model.User, Contract.Model.User>();

            CreateMap<Contract.Model.UserCreator, Db.Model.User>()
                .ForMember(s => s.Password, s => s.Ignore());
        }
    }
}
