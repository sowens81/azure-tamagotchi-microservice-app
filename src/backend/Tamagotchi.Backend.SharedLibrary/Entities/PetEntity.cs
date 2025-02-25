using Tamagotchi.Backend.SharedLibrary.Models;
using Tamagotchi.Backend.SharedLibrary.Utilities;

namespace Tamagotchi.Backend.SharedLibrary.Entities;

class PetEntity
{
    public required string PetId { get; set; } = IdGenerator.GenerateShortId();
    public required string Name { get; set; }
    public required string AnimalType { get; set; }
    public required string color { get; set; }
    public int DaysOld { get; set; } = 0;
    public int Health { get; set; } = 3;
    public bool IsAlive { get; set; } = true;
    public List<Feed> FeedHistroy { get; set; } = new List<Feed>();
    public List<Exercise> ExerciseHistroy { get; set; } = new List<Exercise>();

    public bool FeedPet()
    {
        if (!IsAlive)
            return false;

        int timesFed = FeedHistroy.Count(feed => feed.TimeFeed >= DateTime.Now.AddHours(-24));

        if (Health < 5)
        {
            FeedHistroy.Add(new Feed());
            Health++;
            return true;
        }

        if (Health == 5)
        {
            if (timesFed <= 4)
            {
                FeedHistroy.Add(new Feed());
                return true;
            }
            else
            {
                IsAlive = false; // Pet overfed and dies
                return false;
            }
        }

        return false;
    }

    public bool ExercisePet()
    {
        if (!IsAlive)
            return false;

        int timesExercised = ExerciseHistroy.Count(exersie => exersie.TimeExercised >= DateTime.Now.AddHours(-24));

        if (Health < 5)
        {
            ExerciseHistroy.Add(new Exercise());
            Health++;
            return true;
        }

        if (Health == 5)
        {
            if (timesExercised <= 3)
            {
                ExerciseHistroy.Add(new Exercise());
                return true;
            }
            else
            {
                IsAlive = false; // Pet over exercised and dies
                return false;
            }
        }

        return false;
    }

}
