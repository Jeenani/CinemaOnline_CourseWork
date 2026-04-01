package ru.cinema.main;

import android.content.Intent;
import android.os.Bundle;

import androidx.appcompat.app.AppCompatActivity;
import androidx.fragment.app.Fragment;

import com.google.android.material.bottomnavigation.BottomNavigationView;

import ru.cinema.main.fragment.FilmsFragment;
import ru.cinema.main.fragment.HomeFragment;
import ru.cinema.main.fragment.ProfileFragment;
import ru.cinema.main.fragment.SearchFragment;
import ru.cinema.main.util.SessionManager;

public class MainActivity extends AppCompatActivity {

    private BottomNavigationView bottomNav;
    private SessionManager sessionManager;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        sessionManager = new SessionManager(this);
        bottomNav = findViewById(R.id.bottom_navigation);

        bottomNav.setOnItemSelectedListener(item -> {
            Fragment fragment = null;
            int id = item.getItemId();
            if (id == R.id.nav_home) {
                fragment = new HomeFragment();
            } else if (id == R.id.nav_films) {
                fragment = new FilmsFragment();
            } else if (id == R.id.nav_search) {
                fragment = new SearchFragment();
            } else if (id == R.id.nav_profile) {
                if (!sessionManager.isLoggedIn()) {
                    startActivity(new Intent(this, LoginActivity.class));
                    return false;
                }
                fragment = new ProfileFragment();
            }
            if (fragment != null) {
                getSupportFragmentManager().beginTransaction()
                        .replace(R.id.fragment_container, fragment)
                        .commit();
            }
            return true;
        });

        if (savedInstanceState == null) {
            bottomNav.setSelectedItemId(R.id.nav_home);
        }
    }

    @Override
    protected void onResume() {
        super.onResume();
        // Refresh profile tab if needed
    }

    public void selectTab(int tabId) {
        bottomNav.setSelectedItemId(tabId);
    }
}

