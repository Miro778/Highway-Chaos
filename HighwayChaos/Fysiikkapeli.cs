
using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

// Author: Miro Okkonen - mirmatok@student.jyu.fi
// Last Modified: 17.4.2020 - 11:00
// Muutokset: Lisätty for-silmukka ja autojenkuvat-taulukko. 

public class HighWayChaos : PhysicsGame
{
    private Vector nopeusVasen = new Vector(-275, 0);
    private Vector nopeusOikea = new Vector(275, 0);
    private Vector nopeusAlas = new Vector(0, -350);

    private PhysicsObject pelaajanAuto;

    private Timer aikaLaskuri = new Timer();
    private EasyHighScore parhaatPisteet = new EasyHighScore();
    private int tulos;
    private int vaikeustaso = 3;
    private int[] kaistat = { -200, -25, 150 };
    private string[] autojenkuvat = { "Auto1", "Auto2" }; // Tähän taulukkoon muiden autojen käyttämät kuvat
    private Vector ajastimenPaikka = new Vector(-400, 350);


    public override void Begin()
    {
        // Alkuvalikko, jossa valitaan vaikeustaso, jonka yhteydessä toteutuu Begin.

        Level.BackgroundColor = Color.Black;

        MultiSelectWindow alkuValikko = new MultiSelectWindow("Valitse pelin vaikeustaso aloittaaksesi:", "Helppo", "Keskitaso", "Vaikea", "Poistu");
        alkuValikko.AddItemHandler(0, Helppo);
        alkuValikko.AddItemHandler(1, Keskitaso);
        alkuValikko.AddItemHandler(2, Vaikea);
        alkuValikko.AddItemHandler(3, ConfirmExit);
        alkuValikko.Color = Color.White;


        Add(alkuValikko);

        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }


    // LuoKentta aliohjelma asettaen vaikeustason 1
    private void Helppo() { vaikeustaso = 1; LuoKentta(); }


    // LuoKentta aliohjelma asettaen vaikeustason 2
    private void Keskitaso() { vaikeustaso = 2; LuoKentta(); }


    // LuoKentta aliohjelma asettaen vaikeustason 3
    private void Vaikea() { vaikeustaso = 3; LuoKentta(); }


    // Suorittaa kentän luomiseen liittyvät aliohjelmat ja luo pelaajan auton.
    private void LuoKentta()
    {
        LuoAikaLaskuri();
        SiviiliAutojenTulo();
        AsetaOhjaimet();
        Camera.ZoomToLevel();
        pelaajanAuto = LuoPelaajanAuto(0.0, -250.0);
        AddCollisionHandler(pelaajanAuto, PelaajaTormasi);
        Image taustaKuva = LoadImage("taustaKuva");
        Level.Background.Image = taustaKuva;
    }


    /// Pelaajan auton luominen ominaisuuksineen.
    private PhysicsObject LuoPelaajanAuto(double x, double y)
    {
        PhysicsObject pelaajanAuto = new PhysicsObject(120.0, 240.0);
        Image AutonKuva = LoadImage("PeliAuto");
        pelaajanAuto.X = x;
        pelaajanAuto.Y = y;
        pelaajanAuto.Image = AutonKuva;
        pelaajanAuto.KineticFriction = 1;
        pelaajanAuto.Restitution = 0.2;
        pelaajanAuto.MomentOfInertia = Double.PositiveInfinity;

        Add(pelaajanAuto);
        return pelaajanAuto;
    }


    // Luo siviiliauton ja asettaa sen liikkumaan alaspäin
    private PhysicsObject LuoAuto(int x, int y)
    {
        PhysicsObject auto = new PhysicsObject(120.0, 240.0);
        Image AutonKuva = LoadImage(MaaritaSatunnainenKuva(autojenkuvat));
        auto.X = x;
        auto.Y = y;
        auto.Image = AutonKuva;
        auto.Velocity = nopeusAlas;
        auto.KineticFriction = 1;
        auto.Restitution = 0.8;
        Add(auto);
        return auto;       
    }


    // Palauttaa satunnaisen merkkijonon string[] taulukosta. Käytetään tässä ohjelmassa arpomaan autojen kuvia. 
    private string MaaritaSatunnainenKuva(string[] kuvataulukko)
    {
        bool tuleekoKyseinenAuto = false;
        Random mahdollisuus = new Random();
        for (int i = 0;i < kuvataulukko.Length;i++)
        {
            if (i == kuvataulukko.Length - 1) tuleekoKyseinenAuto = true;
            else
            {
                tuleekoKyseinenAuto = (1 == mahdollisuus.Next(kuvataulukko.Length));

            }
            if (tuleekoKyseinenAuto == true) { return kuvataulukko[i]; }
                
        }
        return kuvataulukko[1];
    }


    // Määrää miten siviiliautoja syntyy millekkin kaistalle
    private void SiviiliAutojenTulo()
    {
        Timer ajastin = new Timer();
        int i = 0;
        bool luodaankoKaksi = false;

        //Autojen ilmestyminen yhdelle kolmesta kaistasta vaikeustason ollessa 1
        while (vaikeustaso == 1) {
            ajastin.Interval = 4;
            ajastin.Timeout += delegate { LuoAuto(RandomGen.SelectOne(kaistat[0], kaistat[1], kaistat[2]), 600); };
            ajastin.Start();
            break; ; }

        //Vaikeustaso 2
        while (vaikeustaso == 2) {
            ajastin.Interval = 3;
            ajastin.Timeout += delegate
            {
                i = RandomGen.SelectOne(0, 1, 2);
                LuoAuto(kaistat[i], 600);
                luodaankoKaksi = RandomGen.SelectOne(false, false, true);
                if (luodaankoKaksi == true)
                {
                    if (i == 0) i = RandomGen.SelectOne(i + 1, i + 2);
                    else if (i == 1) i = RandomGen.SelectOne(i + 1, i - 1);
                    else if (i == 2) i = RandomGen.SelectOne(i - 1, i - 2);
                    LuoAuto(kaistat[i], 600);
                }
            };
            ajastin.Start();
            break; }

        //Vaikeustaso 3
        while (vaikeustaso == 3)
        {
            ajastin.Interval = 2;
            ajastin.Timeout += delegate
            {
                i = RandomGen.SelectOne(0, 1, 2);
                LuoAuto(kaistat[i], 600);
                luodaankoKaksi = RandomGen.SelectOne(false, true);
                if (luodaankoKaksi == true)
                {
                    if (i == 0) i = RandomGen.SelectOne(i + 1, i + 2);
                    else if (i == 1) i = RandomGen.SelectOne(i + 1, i - 1);
                    else if (i == 2) i = RandomGen.SelectOne(i - 1, i - 2);
                    LuoAuto(kaistat[i], 600);
                }
            };
            ajastin.Start();
            break;
        }


    }


    //Määrää toiminnot, jotka tapahtuu ohjaimia painaessa
    private void AsetaOhjaimet()
    {
        // Liike vasemmalle
        Keyboard.Listen(Key.Left, ButtonState.Down,
        LiikutaPelaajaa, null, nopeusVasen);
        Keyboard.Listen(Key.Left, ButtonState.Released,
        LiikutaPelaajaa, null, Vector.Zero);

        // Liike oikealle
        Keyboard.Listen(Key.Right, ButtonState.Down,
        LiikutaPelaajaa, null, nopeusOikea);
        Keyboard.Listen(Key.Right, ButtonState.Released,
        LiikutaPelaajaa, null, Vector.Zero);
    }


    // Ohjainten perusteella tapahtuva liike
    private void LiikutaPelaajaa(Vector nopeus)
    {
        //Pysäyttää liikkeen vasempaan tien reunaan
        if ((nopeus.X < 0) && (pelaajanAuto.Left < Level.Left + 200))
        {
            pelaajanAuto.Velocity = Vector.Zero;
            return;
        }
        //Pysäyttää liikkeen oikeaan tien reunaan
        if ((nopeus.X > 0) && (pelaajanAuto.Right > Level.Right - 240))
        {
            pelaajanAuto.Velocity = Vector.Zero;
            return;
        }
        //Jatkaa liikettä normaalisti, kun auto ei ole liikkumassa tien yli
        pelaajanAuto.Velocity = nopeus;
    }


    // Luo sekuntikellon vasempaan yläkulmaan
    private void LuoAikaLaskuri()
    {
        aikaLaskuri.Reset();
        aikaLaskuri.Start();

        Label aikaNaytto = new Label();
        aikaNaytto.TextColor = Color.Teal;
        aikaNaytto.Color = Color.Black;
        aikaNaytto.DecimalPlaces = 1;
        aikaNaytto.Position = ajastimenPaikka;
        aikaNaytto.BindTo(aikaLaskuri.SecondCounter);
        Add(aikaNaytto);
    }


    // Pelin loppumisen aiheuttava törmäys
    private void PelaajaTormasi(PhysicsObject tormaaja, PhysicsObject Kohde)
    {
        Explosion rajahdys = new Explosion(200);
        rajahdys.Position = pelaajanAuto.Position;
        Add(rajahdys);
        tulos = Convert.ToInt32(aikaLaskuri.SecondCounter);
        LoppuValikko();
    }


    // Valikko pelin loppuessa
    private void LoppuValikko()
    {
            MultiSelectWindow valikko = new MultiSelectWindow("Peli loppui! Selvisit " + tulos + " sekuntia.",
            "Yritä uudelleen", "Parhaat pisteet", "Palaa alkuvalikkoon");
            valikko.ItemSelected += PainettiinValikonNappia;
            Add(valikko);
            valikko.Color = Color.White;
            if (IsPaused) valikko.X = -175;
            IsPaused = true;
        }


        // Loppuvalikon nappien painamisesta tulevat tapahtumat
       private void PainettiinValikonNappia(int valinta)
        {
            switch (valinta)
            {
                case 0:
                    ClearAll();
                    LuoKentta();
                    IsPaused = false;
                    break;
                case 1:
                parhaatPisteet.EnterAndShow(tulos);
                parhaatPisteet.HighScoreWindow.X = 175;
                LoppuValikko();
                break;
                case 2:
                ClearAll();
                Begin();
                IsPaused = false;
                    break;
            }
        }
    }


