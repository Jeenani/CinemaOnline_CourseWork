package ru.cinema.main;

import android.os.Bundle;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.EditText;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.Spinner;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

import java.util.HashMap;
import java.util.List;
import java.util.Map;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;
import ru.cinema.main.api.ApiClient;
import ru.cinema.main.model.ApiResponse;
import ru.cinema.main.model.SubscriptionPlan;
import ru.cinema.main.model.User;
import ru.cinema.main.util.SessionManager;

public class CheckoutActivity extends AppCompatActivity {

    public static final String EXTRA_SUBSCRIPTION_ID = "subscription_id";

    private SessionManager sessionManager;
    private long subscriptionId;
    private SubscriptionPlan selectedPlan;

    private TextView tvPlanName, tvPlanPrice, tvPlanDuration, tvPlanDescription;
    private TextView tvUserName, tvUserEmail;
    private TextView tvTotal, tvError, btnConfirm;
    private Spinner spinnerPaymentMethod;
    private LinearLayout llCardFields;
    private EditText etCardNumber, etExpiry, etCvv;

    private String selectedPaymentMethod = "card";

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_checkout);

        sessionManager = new SessionManager(this);
        subscriptionId = getIntent().getLongExtra(EXTRA_SUBSCRIPTION_ID, -1);

        if (!sessionManager.isLoggedIn()) {
            Toast.makeText(this, getString(R.string.error_auth_required), Toast.LENGTH_SHORT).show();
            finish();
            return;
        }

        initViews();
        fillUserData();
        loadPlanDetails();
    }

    private void initViews() {
        ImageView btnBack = findViewById(R.id.btn_back);
        btnBack.setOnClickListener(v -> finish());

        tvPlanName = findViewById(R.id.tv_plan_name);
        tvPlanPrice = findViewById(R.id.tv_plan_price);
        tvPlanDuration = findViewById(R.id.tv_plan_duration);
        tvPlanDescription = findViewById(R.id.tv_plan_description);
        tvUserName = findViewById(R.id.tv_user_name);
        tvUserEmail = findViewById(R.id.tv_user_email);
        tvTotal = findViewById(R.id.tv_total);
        tvError = findViewById(R.id.tv_error);
        btnConfirm = findViewById(R.id.btn_confirm);
        spinnerPaymentMethod = findViewById(R.id.spinner_payment_method);
        llCardFields = findViewById(R.id.ll_card_fields);
        etCardNumber = findViewById(R.id.et_card_number);
        etExpiry = findViewById(R.id.et_expiry);
        etCvv = findViewById(R.id.et_cvv);

        setupPaymentSpinner();

        btnConfirm.setOnClickListener(v -> confirmPayment());
    }

    private void setupPaymentSpinner() {
        String[] methods = {
                getString(R.string.checkout_payment_card),
                getString(R.string.checkout_payment_sbp),
                getString(R.string.checkout_payment_balance)
        };

        ArrayAdapter<String> adapter = new ArrayAdapter<>(
                this, R.layout.item_spinner, methods);
        adapter.setDropDownViewResource(R.layout.item_spinner);
        spinnerPaymentMethod.setAdapter(adapter);

        spinnerPaymentMethod.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
            @Override
            public void onItemSelected(AdapterView<?> parent, View view, int position, long id) {
                switch (position) {
                    case 0:
                        selectedPaymentMethod = "card";
                        llCardFields.setVisibility(View.VISIBLE);
                        break;
                    case 1:
                        selectedPaymentMethod = "sbp";
                        llCardFields.setVisibility(View.GONE);
                        break;
                    case 2:
                        selectedPaymentMethod = "balance";
                        llCardFields.setVisibility(View.GONE);
                        break;
                }
            }

            @Override
            public void onNothingSelected(AdapterView<?> parent) {}
        });
    }

    private void fillUserData() {
        User user = sessionManager.getUser();
        if (user != null) {
            tvUserName.setText(user.getName() != null ? user.getName() : "—");
            tvUserEmail.setText(user.getEmail() != null ? user.getEmail() : "—");
        }
    }

    private void loadPlanDetails() {
        ApiClient.getService().getSubscriptionPlans().enqueue(new Callback<ApiResponse<List<SubscriptionPlan>>>() {
            @Override
            public void onResponse(Call<ApiResponse<List<SubscriptionPlan>>> call,
                                   Response<ApiResponse<List<SubscriptionPlan>>> response) {
                if (response.isSuccessful() && response.body() != null && response.body().isSuccess()) {
                    List<SubscriptionPlan> plans = response.body().getData();
                    if (plans != null) {
                        for (SubscriptionPlan plan : plans) {
                            if (plan.getId() == subscriptionId) {
                                selectedPlan = plan;
                                displayPlan(plan);
                                return;
                            }
                        }
                        // If not found by ID, use first plan
                        if (!plans.isEmpty()) {
                            selectedPlan = plans.get(0);
                            displayPlan(selectedPlan);
                        }
                    }
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<List<SubscriptionPlan>>> call, Throwable t) {
                tvPlanName.setText("—");
                tvPlanPrice.setText("—");
                tvTotal.setText("—");
            }
        });
    }

    private void displayPlan(SubscriptionPlan plan) {
        tvPlanName.setText(plan.getName());
        tvPlanPrice.setText(String.format("%.2f ₽", plan.getPrice()));
        tvPlanDuration.setText(String.format(getString(R.string.checkout_days), plan.getDurationDays()));
        tvPlanDescription.setText(plan.getDescription() != null ? plan.getDescription() : "");
        tvTotal.setText(String.format("%.2f ₽", plan.getPrice()));
    }

    private void confirmPayment() {
        // Validate card fields if card is selected
        if ("card".equals(selectedPaymentMethod)) {
            String card = etCardNumber.getText().toString().trim();
            String expiry = etExpiry.getText().toString().trim();
            String cvv = etCvv.getText().toString().trim();

            if (card.length() < 13 || expiry.length() < 4 || cvv.length() < 3) {
                tvError.setText(getString(R.string.checkout_error_fill_card));
                tvError.setVisibility(View.VISIBLE);
                return;
            }
        }

        tvError.setVisibility(View.GONE);
        btnConfirm.setEnabled(false);
        btnConfirm.setText(getString(R.string.checkout_processing));

        String token = sessionManager.getToken();
        if (token == null) {
            Toast.makeText(this, getString(R.string.error_auth_required), Toast.LENGTH_SHORT).show();
            return;
        }

        long planId = selectedPlan != null ? selectedPlan.getId() : subscriptionId;

        Map<String, Object> body = new HashMap<>();
        body.put("subscriptionId", planId);
        body.put("paymentMethod", selectedPaymentMethod);

        ApiClient.getService().createPayment(token, body).enqueue(new Callback<ApiResponse<Map<String, Object>>>() {
            @Override
            public void onResponse(Call<ApiResponse<Map<String, Object>>> call,
                                   Response<ApiResponse<Map<String, Object>>> response) {
                if (response.isSuccessful() && response.body() != null && response.body().isSuccess()) {
                    Map<String, Object> payment = response.body().getData();
                    if (payment != null) {
                        Object idObj = payment.get("id");
                        if (idObj != null) {
                            long paymentId = ((Number) idObj).longValue();
                            processPayment(paymentId);
                            return;
                        }
                    }
                }
                btnConfirm.setEnabled(true);
                btnConfirm.setText(getString(R.string.checkout_confirm));
                tvError.setText(getString(R.string.error_payment));
                tvError.setVisibility(View.VISIBLE);
            }

            @Override
            public void onFailure(Call<ApiResponse<Map<String, Object>>> call, Throwable t) {
                btnConfirm.setEnabled(true);
                btnConfirm.setText(getString(R.string.checkout_confirm));
                tvError.setText(getString(R.string.error_network_short));
                tvError.setVisibility(View.VISIBLE);
            }
        });
    }

    private void processPayment(long paymentId) {
        Map<String, Object> body = new HashMap<>();
        body.put("success", true);

        ApiClient.getService().processPayment(paymentId, body).enqueue(new Callback<ApiResponse<Boolean>>() {
            @Override
            public void onResponse(Call<ApiResponse<Boolean>> call, Response<ApiResponse<Boolean>> response) {
                if (response.isSuccessful() && response.body() != null && response.body().isSuccess()) {
                    sessionManager.setHasSubscription(true);
                    Toast.makeText(CheckoutActivity.this, getString(R.string.subscription_success), Toast.LENGTH_LONG).show();
                    setResult(RESULT_OK);
                    finish();
                } else {
                    btnConfirm.setEnabled(true);
                    btnConfirm.setText(getString(R.string.checkout_confirm));
                    tvError.setText(getString(R.string.error_payment));
                    tvError.setVisibility(View.VISIBLE);
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<Boolean>> call, Throwable t) {
                // Fallback — mark locally
                sessionManager.setHasSubscription(true);
                Toast.makeText(CheckoutActivity.this, getString(R.string.subscription_success), Toast.LENGTH_LONG).show();
                setResult(RESULT_OK);
                finish();
            }
        });
    }
}

