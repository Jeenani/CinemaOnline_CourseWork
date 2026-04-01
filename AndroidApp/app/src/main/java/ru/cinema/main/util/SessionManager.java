package ru.cinema.main.util;

import android.content.Context;
import android.content.SharedPreferences;

import com.google.gson.Gson;

import ru.cinema.main.model.SubscriptionInfo;
import ru.cinema.main.model.User;

public class SessionManager {

    private static final String PREF_NAME = "cinema_session";
    private static final String KEY_TOKEN = "token";
    private static final String KEY_USER_ID = "user_id";
    private static final String KEY_USER_EMAIL = "user_email";
    private static final String KEY_USER_NAME = "user_name";
    private static final String KEY_USER_ROLE = "user_role";
    private static final String KEY_HAS_SUBSCRIPTION = "has_subscription";
    private static final String KEY_SUBSCRIPTION_JSON = "subscription_json";
    private static final String KEY_IS_LOGGED_IN = "is_logged_in";

    private final SharedPreferences prefs;
    private final Gson gson = new Gson();

    public SessionManager(Context context) {
        prefs = context.getSharedPreferences(PREF_NAME, Context.MODE_PRIVATE);
    }

    public void saveUser(User user, String token) {
        SharedPreferences.Editor editor = prefs.edit();
        editor.putString(KEY_TOKEN, token);
        editor.putLong(KEY_USER_ID, user.getId());
        editor.putString(KEY_USER_EMAIL, user.getEmail());
        editor.putString(KEY_USER_NAME, user.getName());
        editor.putString(KEY_USER_ROLE, user.getRole());
        editor.putBoolean(KEY_HAS_SUBSCRIPTION, user.isHasSubscription());
        editor.putBoolean(KEY_IS_LOGGED_IN, true);
        if (user.getSubscription() != null) {
            editor.putString(KEY_SUBSCRIPTION_JSON, gson.toJson(user.getSubscription()));
        } else {
            editor.remove(KEY_SUBSCRIPTION_JSON);
        }
        editor.apply();
    }

    public boolean isLoggedIn() {
        return prefs.getBoolean(KEY_IS_LOGGED_IN, false);
    }

    public String getToken() {
        return prefs.getString(KEY_TOKEN, null);
    }

    public User getUser() {
        if (!isLoggedIn()) return null;
        User user = new User();
        user.setId(prefs.getLong(KEY_USER_ID, 0));
        user.setEmail(prefs.getString(KEY_USER_EMAIL, ""));
        user.setName(prefs.getString(KEY_USER_NAME, ""));
        user.setRole(prefs.getString(KEY_USER_ROLE, "user"));
        user.setHasSubscription(prefs.getBoolean(KEY_HAS_SUBSCRIPTION, false));
        String subJson = prefs.getString(KEY_SUBSCRIPTION_JSON, null);
        if (subJson != null) {
            user.setSubscription(gson.fromJson(subJson, SubscriptionInfo.class));
        }
        return user;
    }

    public void setHasSubscription(boolean has) {
        prefs.edit().putBoolean(KEY_HAS_SUBSCRIPTION, has).apply();
    }

    public void logout() {
        prefs.edit().clear().apply();
    }
}

