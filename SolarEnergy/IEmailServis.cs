namespace SolarEnergy;

public interface IEmailServis
{
    Task PosaljiEmail(string email, Izvjestaj izvjestaj);
}