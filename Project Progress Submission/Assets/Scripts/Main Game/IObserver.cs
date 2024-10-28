using UnityEngine;

//The Observer will take note of what actions the user does, and awards achievements or fills an achievement guage.

public interface IObserver
{
    void addOserver();

    void removeObserver();

    void notifyObserver();
}
