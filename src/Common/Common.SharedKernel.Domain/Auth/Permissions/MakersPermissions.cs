using System.Collections.ObjectModel;

namespace Common.SharedKernel.Domain.Auth.Permissions;

public static class MakersPermissions
{
    private static readonly MakersPermission[] AllPermissions =
    [
        //Users
        new ("View Users", MakersModules.FVP, MakersDomains.Users, MakersResources.Users, MakersActions.View),
        new ("Search Users", MakersModules.FVP, MakersDomains.Users, MakersResources.Users, MakersActions.Search),
        new ("Create Users", MakersModules.FVP, MakersDomains.Users, MakersResources.Users, MakersActions.Create),
        new ("Update Users", MakersModules.FVP, MakersDomains.Users, MakersResources.Users, MakersActions.Update),
        new ("Delete Users", MakersModules.FVP, MakersDomains.Users, MakersResources.Users, MakersActions.Delete),

        //Associate Activates
        new ("View Activates", MakersModules.FVP, MakersDomains.Associate, MakersResources.Activates, MakersActions.View),
        new ("Search Activates", MakersModules.FVP, MakersDomains.Associate, MakersResources.Activates, MakersActions.Search),
        new ("Create Activates", MakersModules.FVP, MakersDomains.Associate, MakersResources.Activates, MakersActions.Create),
        new ("Update Activates", MakersModules.FVP, MakersDomains.Associate, MakersResources.Activates, MakersActions.Update),
        new ("Delete Activates", MakersModules.FVP, MakersDomains.Associate, MakersResources.Activates, MakersActions.Delete),

        //Associate Pension Requirements
        new ("View Pension Requirements", MakersModules.FVP, MakersDomains.Associate, MakersResources.PensionRequirements, MakersActions.View),
        new ("Search Pension Requirements", MakersModules.FVP, MakersDomains.Associate, MakersResources.PensionRequirements, MakersActions.Search),
        new ("Create Pension Requirements", MakersModules.FVP, MakersDomains.Associate, MakersResources.PensionRequirements, MakersActions.Create),
        new ("Update Pension Requirements", MakersModules.FVP, MakersDomains.Associate, MakersResources.PensionRequirements, MakersActions.Update),
        new ("Delete Pension Requirements", MakersModules.FVP, MakersDomains.Associate, MakersResources.PensionRequirements, MakersActions.Delete),

        //Associate Configuration Parameters
        new ("View Configuration Parameters", MakersModules.FVP, MakersDomains.Associate, MakersResources.ConfigurationParameters, MakersActions.View),
        new ("Search Configuration Parameters", MakersModules.FVP, MakersDomains.Associate, MakersResources.ConfigurationParameters, MakersActions.Search),
        new ("Create Configuration Parameters", MakersModules.FVP, MakersDomains.Associate, MakersResources.ConfigurationParameters, MakersActions.Create),
        new ("Update Configuration Parameters", MakersModules.FVP, MakersDomains.Associate, MakersResources.ConfigurationParameters, MakersActions.Update),
        new ("Delete Configuration Parameters", MakersModules.FVP, MakersDomains.Associate, MakersResources.ConfigurationParameters, MakersActions.Delete),

        //Operations Auxiliary Informations
        new ("View Auxiliary Informations", MakersModules.FVP, MakersDomains.Operations, MakersResources.AuxiliaryInformations, MakersActions.View),
        new ("Search Auxiliary Informations", MakersModules.FVP, MakersDomains.Operations, MakersResources.AuxiliaryInformations, MakersActions.Search),
        new ("Create Auxiliary Informations", MakersModules.FVP, MakersDomains.Operations, MakersResources.AuxiliaryInformations, MakersActions.Create),
        new ("Update Auxiliary Informations", MakersModules.FVP, MakersDomains.Operations, MakersResources.AuxiliaryInformations, MakersActions.Update),
        new ("Delete Auxiliary Informations", MakersModules.FVP, MakersDomains.Operations, MakersResources.AuxiliaryInformations, MakersActions.Delete),

        //Operations Client Operations
        new ("View Client Operations", MakersModules.FVP, MakersDomains.Operations, MakersResources.ClientOperations, MakersActions.View),
        new ("Search Client Operations", MakersModules.FVP, MakersDomains.Operations, MakersResources.ClientOperations, MakersActions.Search),
        new ("Create Client Operations", MakersModules.FVP, MakersDomains.Operations, MakersResources.ClientOperations, MakersActions.Create),
        new ("Update Client Operations", MakersModules.FVP, MakersDomains.Operations, MakersResources.ClientOperations, MakersActions.Update),
        new ("Delete Client Operations", MakersModules.FVP, MakersDomains.Operations, MakersResources.ClientOperations, MakersActions.Delete),

        //Operations Configuration Parameters
        new ("View Configuration Parameters", MakersModules.FVP, MakersDomains.Operations, MakersResources.ConfigurationParameters, MakersActions.View),
        new ("Search Configuration Parameters", MakersModules.FVP, MakersDomains.Operations, MakersResources.ConfigurationParameters, MakersActions.Search),
        new ("Create Configuration Parameters", MakersModules.FVP, MakersDomains.Operations, MakersResources.ConfigurationParameters, MakersActions.Create),
        new ("Update Configuration Parameters", MakersModules.FVP, MakersDomains.Operations, MakersResources.ConfigurationParameters, MakersActions.Update),
        new ("Delete Configuration Parameters", MakersModules.FVP, MakersDomains.Operations, MakersResources.ConfigurationParameters, MakersActions.Delete),
    ];

    public static IReadOnlyList<MakersPermission> All { get; } = new ReadOnlyCollection<MakersPermission>(AllPermissions);
}
