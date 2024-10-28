public interface ISettings
{
    void syncSettings();

    void load(string data);

    string save();

    void reseatUI(); //Yes, reseat, because we're seating the UI values again

    void sameSettingsCheck();
}