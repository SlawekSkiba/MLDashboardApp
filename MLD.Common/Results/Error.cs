namespace MLD.Application.Results;
public record Error(string Message);

public class NotFound
{
    private static NotFound? _instance = null;
    public static NotFound Instance()
    {
        if(_instance == null)
        {
            _instance = new NotFound();
        }
        return _instance;
    }
}
