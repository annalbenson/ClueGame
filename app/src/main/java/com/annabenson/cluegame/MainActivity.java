package com.annabenson.cluegame;

import android.os.Bundle;

import com.annabenson.cluegame.cards.*;
import com.google.android.material.snackbar.Snackbar;

import androidx.appcompat.app.AppCompatActivity;

import android.view.View;

import androidx.navigation.NavController;
import androidx.navigation.Navigation;
import androidx.navigation.ui.AppBarConfiguration;
import androidx.navigation.ui.NavigationUI;

import java.util.ArrayList;
import java.util.List;

import com.annabenson.cluegame.databinding.ActivityMainBinding;

import android.view.Menu;
import android.view.MenuItem;

public class MainActivity extends AppCompatActivity {

    private AppBarConfiguration appBarConfiguration;
    private ActivityMainBinding binding;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        binding = ActivityMainBinding.inflate(getLayoutInflater());
        setContentView(binding.getRoot());

        setSupportActionBar(binding.toolbar);

        NavController navController = Navigation.findNavController(this, R.id.nav_host_fragment_content_main);
        appBarConfiguration = new AppBarConfiguration.Builder(navController.getGraph()).build();
        NavigationUI.setupActionBarWithNavController(this, navController, appBarConfiguration);

        binding.fab.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                Snackbar.make(view, "Replace with your own action", Snackbar.LENGTH_LONG)
                        .setAction("Action", null).show();
            }
        });
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.menu_main, menu);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        // Handle action bar item clicks here. The action bar will
        // automatically handle clicks on the Home/Up button, so long
        // as you specify a parent activity in AndroidManifest.xml.
        int id = item.getItemId();

        //noinspection SimplifiableIfStatement
        if (id == R.id.action_settings) {
            return true;
        }

        return super.onOptionsItemSelected(item);
    }

    @Override
    public boolean onSupportNavigateUp() {
        NavController navController = Navigation.findNavController(this, R.id.nav_host_fragment_content_main);
        return NavigationUI.navigateUp(navController, appBarConfiguration)
                || super.onSupportNavigateUp();
    }


    public void startGame() {

        // initialize all Characters, Rooms, and Weapons

        List<Card> CharacterCards = initializeCharacters();
        List<Card> RoomCards = initializeRooms();
        List<Card> WeaponCards = initializeWeapons();

        // create solution; get random from each


        // shuffle remaining cards
        List<Card> Deck = new ArrayList<>();
        Deck.addAll(CharacterCards);
        Deck.addAll(RoomCards);
        Deck.addAll(WeaponCards);

        // distribute cards to player and npcs

        // ask player who they want to be

    }

    public ArrayList<Card> initializeCharacters(){

        ArrayList<Card> CharacterCards = new ArrayList<Card>();

        CharacterCards.add(new ColonelMustard()); // 1
        CharacterCards.add(new MissScarlet()); // 2
        CharacterCards.add(new MrGreen()); // 3
        CharacterCards.add(new MrsPeacock()); // 4
        CharacterCards.add(new MrsWhite()); // 5
        CharacterCards.add(new ProfessorPlum()); // 6

        return CharacterCards;
    }

    public ArrayList<Card> initializeRooms(){

        ArrayList<Card> RoomCards = new ArrayList<Card>();

        RoomCards.add(new Ballroom()); // 1
        RoomCards.add(new BilliardRoom()); // 2
        RoomCards.add(new Cellar()); // 3
        RoomCards.add(new Conservatory()); // 4
        RoomCards.add(new DiningRoom()); // 5
        RoomCards.add(new Hall()); // 6
        RoomCards.add(new Kitchen()); // 7
        RoomCards.add(new Library()); // 8
        RoomCards.add(new Lounge()); // 9
        RoomCards.add(new Study()); // 10

        return RoomCards;
    }

    public ArrayList<Card> initializeWeapons(){

        ArrayList<Card> WeaponCards = new ArrayList<Card>();

        WeaponCards.add(new Candlestick()); // 1
        WeaponCards.add(new Knife()); // 2
        WeaponCards.add(new LeadPipe()); // 3
        WeaponCards.add(new Revolver()); // 4
        WeaponCards.add(new Rope()); // 5
        WeaponCards.add(new Wrench()); // 6

        return WeaponCards;
    }
}