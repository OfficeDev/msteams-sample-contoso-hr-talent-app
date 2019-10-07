using Autofac;
using AutoMapper;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Internals.Fibers;
using TeamsTalentMgmtAppV3.Dialogs;
using TeamsTalentMgmtAppV3.Services;
using TeamsTalentMgmtAppV3.Services.Interfaces;
using TeamsTalentMgmtAppV3.Services.MessagingExtension;
using TeamTalentMgmtApp.Shared.AutoMapper;
using TeamTalentMgmtApp.Shared.Services.Data;
using TeamTalentMgmtApp.Shared.Services.Interfaces;

namespace TeamsTalentMgmtAppV3
{
    public sealed class TeamsTalentMgmtAppModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            // register the top level dialog and dialog factory
            builder.RegisterType<MainDialog>()
                .As<IDialog<object>>()
                .InstancePerDependency();

            builder.RegisterType<DialogFactory>()
                .Keyed<IDialogFactory>(FiberModule.Key_DoNotSerialize)
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            // register other dialogs we use
            builder.RegisterType<CandidateDetailsDialog>().InstancePerDependency();
            builder.RegisterType<TopCandidatesDialog>().InstancePerDependency();
            builder.RegisterType<OpenPositionsDialog>().InstancePerDependency();
            builder.RegisterType<PositionsDetailsDialog>().InstancePerDependency();
            builder.RegisterType<NewJobPostingDialog>().InstancePerDependency();
            builder.RegisterType<CandidateSummaryDialog>().InstancePerDependency();
            builder.RegisterType<NewTeamDialog>().InstancePerDependency();
            builder.RegisterType<HelpDialog>().InstancePerDependency();
            builder.RegisterType<SignOutDialog>().InstancePerDependency();

            // register services
            builder.Register(_ => new DatabaseContext())
                .Keyed<DatabaseContext>(FiberModule.Key_DoNotSerialize)
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<MessagingExtensionService>()
                .Keyed<IMessagingExtensionService>(FiberModule.Key_DoNotSerialize)
                .AsImplementedInterfaces()
                .InstancePerDependency();

            builder.RegisterType<MessagingExtensionActionsService>()
                .Keyed<IMessagingExtensionActionsService>(FiberModule.Key_DoNotSerialize)
                .AsImplementedInterfaces()
                .InstancePerDependency();
            
            builder.RegisterType<TemplateService>()
                .Keyed<ITemplateService>(FiberModule.Key_DoNotSerialize)
                .AsImplementedInterfaces()
                .InstancePerDependency(); 
            
            builder.RegisterType<CandidateService>()
                .Keyed<ICandidateService>(FiberModule.Key_DoNotSerialize)
                .AsImplementedInterfaces()
                .InstancePerDependency();

            builder.RegisterType<NotificationService>()
                .Keyed<INotificationService>(FiberModule.Key_DoNotSerialize)
                .AsImplementedInterfaces()
                .InstancePerDependency();

            builder.RegisterType<RecruiterService>()
                .Keyed<IRecruiterService>(FiberModule.Key_DoNotSerialize)
                .AsImplementedInterfaces()
                .InstancePerDependency();
            
            builder.RegisterType<LocationService>()
                .Keyed<ILocationService>(FiberModule.Key_DoNotSerialize)
                .AsImplementedInterfaces()
                .InstancePerDependency();
            
            builder.RegisterType<InterviewService>()
                .Keyed<IInterviewService>(FiberModule.Key_DoNotSerialize)
                .AsImplementedInterfaces()
                .InstancePerDependency();
            
            builder.RegisterType<PositionService>()
                .Keyed<IPositionService>(FiberModule.Key_DoNotSerialize)
                .AsImplementedInterfaces()
                .InstancePerDependency();
            
            builder.RegisterType<BotService>()
                .Keyed<IBotService>(FiberModule.Key_DoNotSerialize)
                .AsImplementedInterfaces()
                .InstancePerDependency();
            
            builder.RegisterType<GraphApiService>()
                .Keyed<IGraphApiService>(FiberModule.Key_DoNotSerialize)
                .AsImplementedInterfaces()
                .InstancePerDependency();

            // automapper
            builder.Register(c => new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<TeamsTalentAppBaseProfile>();
                    cfg.AddProfile(new TeamsTalentMgmtAppProfile(c.Resolve<ITemplateService>()));
                }))
               .AsSelf()
               .SingleInstance();

            builder.Register(c => c.Resolve<MapperConfiguration>()
                    .CreateMapper(c.Resolve))
                .Keyed<IMapper>(FiberModule.Key_DoNotSerialize)
                .As<IMapper>()
                .InstancePerLifetimeScope();
        }
    }
}