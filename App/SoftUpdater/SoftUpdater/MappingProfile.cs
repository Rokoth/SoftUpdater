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

            CreateMap<Db.Model.UserHistory, Contract.Model.UserHistory>();

            CreateMap<Contract.Model.UserUpdater, Db.Model.User>()
                .ForMember(s => s.Password, s => s.Ignore());

            CreateMap<Db.Model.Client, Contract.Model.Client>();

            CreateMap<Contract.Model.ClientCreator, Db.Model.Client>()
                .ForMember(s => s.Password, s => s.Ignore());

            CreateMap<Db.Model.ClientHistory, Contract.Model.ClientHistory>();

            CreateMap<Contract.Model.ClientUpdater, Db.Model.Client>()
                .ForMember(s => s.Password, s => s.Ignore());

            CreateMap<Db.Model.Release, Contract.Model.Release>();
            CreateMap<Contract.Model.ReleaseCreator, Db.Model.Release>();
            CreateMap<Contract.Model.ReleaseUpdater, Db.Model.Release>();

            CreateMap<Db.Model.ReleaseArchitect, Contract.Model.ReleaseArchitect>();
            CreateMap<Contract.Model.ReleaseArchitectCreator, Db.Model.ReleaseArchitect>();
            CreateMap<Contract.Model.ReleaseArchitectUpdater, Db.Model.ReleaseArchitect>();

            CreateMap<Db.Model.LoadHistory, Contract.Model.LoadHistory>();
            CreateMap<Contract.Model.LoadHistoryCreator, Db.Model.LoadHistory>();
        }
    }
}
