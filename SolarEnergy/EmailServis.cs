namespace SolarEnergy
{
    public class EmailServis : IEmailServis
    {
        public Task PosaljiEmail(string email, Izvjestaj izvjestaj)
        {
            return Task.CompletedTask;
        }
    }
}