using UnityEngine;
using System;
using NUnit.Framework;
using System.Collections.Generic;
using Assets._00_Scripts.locations.industries;
using UnityEngine.Rendering;
public class Game : MonoBehaviour
{
    private const int secondsInDay = 86400; // 24 hours

    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private BuildingSystem buildingSystem;

    [Header("Day/Night Timing")]
    [SerializeField] private float dayStart;
    [SerializeField] private float eveningStart;
    [SerializeField] private float nightStart;

    [Header("Audio")]
    [SerializeField] private AudioSource music;
    [SerializeField] private List<AudioSource> soundEffects;

    [Header("Economy")]
    [SerializeField] private float nonstopMultiplier;
    [SerializeField] private float linearMultiplier;
    public int sliderScale = 17;

    private float lastMusicVolume;
    private float lastSFXVolume;
    private List<Industry> industries = new List<Industry>();
    private List<City> cities;
    private double globalTime;
    private float gameTime;
    private int day;
    private DayPhase dayPhase;
    public float TimeMultiplier {  get; private set; }
    public Player Player { get => player; private set => player = value; }
    public InputManager InputManager { get => inputManager; private set => inputManager = value; }
    public DayPhase DayPhase
    {
        get
        {
            return dayPhase;
        }
        private set
        {
            if (dayPhase == value) return;
            dayPhase = value;
            if (BuildingSystem != null)
            {
                foreach (ILocation location in BuildingSystem.Grid.Locations)
                {
                    if (location is City city)
                    {
                        city.DayPhase = dayPhase;
                    }
                }
            }
        }
    }

    public BuildingSystem BuildingSystem { get => buildingSystem; set => buildingSystem = value; }
    public AudioSource Music { get => music; set => music = value; }
    public List<AudioSource> SoundEffects { get => soundEffects; set => soundEffects = value; }

    public delegate void TimeChangedEventHandler(object sender, TimeChangedEventArgs e);
    public event TimeChangedEventHandler TimeChanged;

    private void Awake()
    {
        if (BuildingSystem != null)
        {
            BuildingSystem.BuyRequest += BuildingSystem_BuyRequest;
            BuildingSystem.AnyBusMileageChanged += BuildingSystem_AnyBusMileageChanged;
            BuildingSystem.Grid.LocationsRegistered += Grid_LocationsRegistered;
            BuildingSystem.CancelCharge += BuildingSystem_CancelCharge;
        }
    }

    private void BuildingSystem_CancelCharge(object sender, CancelChargeEventArgs e)
    {
        Player.LoseMoney(e.Penalty);
    }

    private void Start()
    {
        globalTime = 0;
        gameTime = 0;
        day = 1;
        TimeMultiplier = 144; // 1 day = 10 irl minutes
    }

    private void Grid_LocationsRegistered(object sender, LocationsRegisteredEventArgs e)
    {
        cities = new List<City>();
        foreach (ILocation location in e.RegisteredLocations)
        {
            if (location is Industry industry)
            {
                industry.GetTime += Industry_GetTime;
                industry.Produced += Player.Industry_Produced;
                industry.GetPhaseTimes += Industry_GetPhaseTimes;
                industries.Add(industry);
            } else if(location is City city)
            {
                cities.Add(city);
            }
        }

        foreach (Road road in e.RegisteredRoads)
        {
            road.RoadStateChangedEventHandler += buildingSystem.Road_RoadStateChangedEventHandler;
        }
        buildingSystem.PaintRegisteredRoads(e.RegisteredRoads);
        buildingSystem.RegisterCityRoadsAsJokers(e.RegisteredRoads);
        buildingSystem.LocationsRegistered(e.RegisteredLocations);
    }

    private void Industry_GetPhaseTimes(object sender, GetPhaseTimesEventArgs e)
    {
        e.DayStart = dayStart;
        e.EveningStart = eveningStart;
        e.NightStart = nightStart;
    }

    private void Industry_GetTime(object sender, GetTimeEventArgs e)
    {
        e.Time = gameTime;
    }

    private void BuildingSystem_AnyBusMileageChanged(object sender, MileageChangedEventArgs e)
    {
        double dist = e.NewAmount;
        bool nonStop = e.NonStop;
        double cost;
        if (nonStop) // nonstop more expensive
        {
            cost = dist * nonstopMultiplier;
            player.IncreaseTax(cost);
        } else
        {
            cost = dist * linearMultiplier;
            player.IncreaseTax(cost);
        }
    }

    private void BuildingSystem_BuyRequest(object sender, BuyRequestEventArgs e)
    {
        if (e.Deduct)
        {
            if (Player.CanAfford(e.Cost))
            {
                Player.LoseMoney(e.Cost);
            } else
            {
                // TODO notification
            }
        }
        if (Player.CanAfford(e.Cost))
        {
            e.IsApproved = true;
        }
        else
        {
            e.IsApproved = false;
        }
    }

    public void Update()
    {
        globalTime += Time.deltaTime * TimeMultiplier;
        gameTime += Time.deltaTime * TimeMultiplier;
        ConvertTime();
        TimeChanged?.Invoke(this, new TimeChangedEventArgs { NewTime = gameTime, Day = day });
        if (gameTime >= nightStart || gameTime < dayStart)
        {
            DayPhase = DayPhase.NIGHT;
        } else if (gameTime >= eveningStart)
        {
            DayPhase = DayPhase.EVENING;
        } else
        {
            DayPhase = DayPhase.DAY;
        }
        foreach (Industry industry in industries)
        {
            industry.UpdateShifts(gameTime);
            if (industry is Mill mill)
            {
                mill.RotateFan(Time.deltaTime);
            }
        }
        
    }

    public void ConvertTime()
    {
        if (gameTime >= secondsInDay)
        {
            gameTime -= secondsInDay;
            day++;

            if (player != null)
            {
                if (day >= player.NextTaxDay)
                {
                    player.PayTaxes();
                }
            }
            else
            {
                Debug.LogError("Game: Player reference is null!");
            }
        }
    }

    public void MuteMusic()
    {
        if (music.volume > 0)
        {
            lastMusicVolume = music.volume;
            music.volume = 0;
        }
        else
        {
            music.volume = lastMusicVolume > 0 ? lastMusicVolume : 1f;
        }
    }

    public void ChangeMusicVolume(float volume)
    {
        music.volume = volume / sliderScale;
        if (volume > 0) lastMusicVolume = volume / sliderScale;
    }

    public void ChangeSFXVolume(float volume)
    {
        foreach (AudioSource sfx in soundEffects)
        {
            if (sfx != null) sfx.volume = volume / sliderScale;
        }
        if (volume > 0) lastSFXVolume = volume / sliderScale;
    }

    public void MuteSFX()
    {
        if (soundEffects.Count == 0) return;
        float currentVolume = soundEffects[0].volume;
        if (currentVolume > 0)
        {
            lastSFXVolume = currentVolume;
            foreach (AudioSource sfx in soundEffects)
                if (sfx != null) sfx.volume = 0;
        }
        else
        {
            float restoreVolume = lastSFXVolume > 0 ? lastSFXVolume : 1f;
            foreach (AudioSource sfx in soundEffects)
                if (sfx != null) sfx.volume = restoreVolume;
        }
    }
}
