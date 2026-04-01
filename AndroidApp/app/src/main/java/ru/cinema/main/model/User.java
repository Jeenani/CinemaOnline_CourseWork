package ru.cinema.main.model;

import com.google.gson.annotations.SerializedName;

public class User {
    private long id;
    private String email;
    private String name;
    private String role;
    @SerializedName("hasSubscription")
    private boolean hasSubscription;
    private SubscriptionInfo subscription;

    public User() {}

    public long getId() { return id; }
    public void setId(long id) { this.id = id; }
    public String getEmail() { return email; }
    public void setEmail(String email) { this.email = email; }
    public String getName() { return name; }
    public void setName(String name) { this.name = name; }
    public String getRole() { return role; }
    public void setRole(String role) { this.role = role; }
    public boolean isHasSubscription() { return hasSubscription; }
    public void setHasSubscription(boolean hasSubscription) { this.hasSubscription = hasSubscription; }
    public SubscriptionInfo getSubscription() { return subscription; }
    public void setSubscription(SubscriptionInfo subscription) { this.subscription = subscription; }
}

