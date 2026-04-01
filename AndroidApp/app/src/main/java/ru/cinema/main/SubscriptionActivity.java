package ru.cinema.main;

import android.content.Intent;
import android.os.Bundle;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

import java.util.List;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;
import ru.cinema.main.api.ApiClient;
import ru.cinema.main.model.ApiResponse;
import ru.cinema.main.model.SubscriptionPlan;
import ru.cinema.main.util.SessionManager;

public class SubscriptionActivity extends AppCompatActivity {

    private SessionManager sessionManager;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_subscription);

        sessionManager = new SessionManager(this);

        ImageView btnBack = findViewById(R.id.btn_back);
        btnBack.setOnClickListener(v -> finish());

        TextView btnSubscribePremium = findViewById(R.id.btn_subscribe_premium);
        TextView btnSubscribeBasic = findViewById(R.id.btn_subscribe_basic);
        TextView btnChooseSub = findViewById(R.id.btn_choose_sub);

        // Load plans from API and bind to buttons
        loadPlans(btnSubscribePremium, btnSubscribeBasic);

        btnChooseSub.setOnClickListener(v -> {
            Toast.makeText(this, getString(R.string.choose_plan_above), Toast.LENGTH_SHORT).show();
        });
    }

    private void loadPlans(TextView btnPremium, TextView btnBasic) {
        ApiClient.getService().getSubscriptionPlans().enqueue(new Callback<ApiResponse<List<SubscriptionPlan>>>() {
            @Override
            public void onResponse(Call<ApiResponse<List<SubscriptionPlan>>> call, Response<ApiResponse<List<SubscriptionPlan>>> response) {
                if (response.isSuccessful() && response.body() != null && response.body().isSuccess()) {
                    List<SubscriptionPlan> plans = response.body().getData();
                    if (plans != null && !plans.isEmpty()) {
                        SubscriptionPlan first = plans.get(0);
                        btnPremium.setOnClickListener(v -> openCheckout(first.getId()));

                        if (plans.size() > 1) {
                            SubscriptionPlan second = plans.get(1);
                            btnBasic.setOnClickListener(v -> openCheckout(second.getId()));
                        }
                    }
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<List<SubscriptionPlan>>> call, Throwable t) {
                btnPremium.setOnClickListener(v -> openCheckout(1));
                btnBasic.setOnClickListener(v -> openCheckout(2));
            }
        });
    }

    private void openCheckout(long subscriptionId) {
        if (!sessionManager.isLoggedIn()) {
            Toast.makeText(this, getString(R.string.error_auth_required), Toast.LENGTH_SHORT).show();
            startActivity(new Intent(this, LoginActivity.class));
            return;
        }
        Intent intent = new Intent(this, CheckoutActivity.class);
        intent.putExtra(CheckoutActivity.EXTRA_SUBSCRIPTION_ID, subscriptionId);
        startActivity(intent);
    }
}
