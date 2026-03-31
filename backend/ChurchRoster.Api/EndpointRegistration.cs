using ChurchRoster.Api.Endpoints.V1;

namespace ChurchRoster.Api
{
    public static  class EndpointRegistration
    {
        public static void RegisterEndpoints(this IEndpointRouteBuilder app)
        {

          
            app.MapAuthEndpoints();
            app.MapMemberEndpoints();
            app.MapSkillEndpoints();
            app.MapTaskEndpoints();
            app.MapAssignmentEndpoints();

        }
    }
}
