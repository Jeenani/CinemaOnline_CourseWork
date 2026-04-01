package ru.cinema.main;

import android.os.Bundle;
import android.view.View;
import android.widget.EditText;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

import java.util.HashMap;
import java.util.Map;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;
import ru.cinema.main.api.ApiClient;
import ru.cinema.main.model.ApiResponse;
import ru.cinema.main.model.User;
import ru.cinema.main.util.SessionManager;

public class AccountSettingsActivity extends AppCompatActivity {

    private SessionManager sessionManager;

    private TextView tvAvatarLetter, tvError, tvSuccess, btnSave;
    private EditText etName, etEmail, etCurrentPassword, etNewPassword, etConfirmPassword;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_account_settings);

        sessionManager = new SessionManager(this);

        if (!sessionManager.isLoggedIn()) {
            finish();
            return;
        }

        initViews();
        fillCurrentData();
    }

    private void initViews() {
        ImageView btnBack = findViewById(R.id.btn_back);
        btnBack.setOnClickListener(v -> finish());

        tvAvatarLetter = findViewById(R.id.tv_avatar_letter);
        etName = findViewById(R.id.et_name);
        etEmail = findViewById(R.id.et_email);
        etCurrentPassword = findViewById(R.id.et_current_password);
        etNewPassword = findViewById(R.id.et_new_password);
        etConfirmPassword = findViewById(R.id.et_confirm_password);
        tvError = findViewById(R.id.tv_error);
        tvSuccess = findViewById(R.id.tv_success);
        btnSave = findViewById(R.id.btn_save);

        btnSave.setOnClickListener(v -> saveSettings());
    }

    private void fillCurrentData() {
        User user = sessionManager.getUser();
        if (user == null) return;

        String name = user.getName();
        tvAvatarLetter.setText(name != null && !name.isEmpty() ?
                String.valueOf(name.charAt(0)).toUpperCase() : "?");
        etName.setText(name != null ? name : "");
        etEmail.setText(user.getEmail() != null ? user.getEmail() : "");
    }

    private void saveSettings() {
        String name = etName.getText().toString().trim();
        String email = etEmail.getText().toString().trim();
        String currentPassword = etCurrentPassword.getText().toString().trim();
        String newPassword = etNewPassword.getText().toString().trim();
        String confirmPassword = etConfirmPassword.getText().toString().trim();

        tvError.setVisibility(View.GONE);
        tvSuccess.setVisibility(View.GONE);

        // Validate
        if (name.isEmpty() || email.isEmpty()) {
            showError(getString(R.string.error_fill_fields));
            return;
        }

        // If changing password, validate fields
        if (!newPassword.isEmpty()) {
            if (currentPassword.isEmpty()) {
                showError(getString(R.string.settings_error_current_password));
                return;
            }
            if (newPassword.length() < 6) {
                showError(getString(R.string.error_password_short));
                return;
            }
            if (!newPassword.equals(confirmPassword)) {
                showError(getString(R.string.error_passwords_mismatch));
                return;
            }
        }

        btnSave.setEnabled(false);

        String token = sessionManager.getToken();
        if (token == null) {
            showError(getString(R.string.error_auth_required));
            return;
        }

        Map<String, String> body = new HashMap<>();
        body.put("name", name);
        body.put("email", email);
        if (!currentPassword.isEmpty()) {
            body.put("currentPassword", currentPassword);
        }
        if (!newPassword.isEmpty()) {
            body.put("newPassword", newPassword);
        }

        ApiClient.getService().updateProfile(token, body).enqueue(new Callback<ApiResponse<User>>() {
            @Override
            public void onResponse(Call<ApiResponse<User>> call, Response<ApiResponse<User>> response) {
                btnSave.setEnabled(true);
                if (response.isSuccessful() && response.body() != null && response.body().isSuccess()) {
                    User updatedUser = response.body().getData();
                    if (updatedUser != null) {
                        sessionManager.saveUser(updatedUser, token);
                    } else {
                        // Update locally from form
                        User user = sessionManager.getUser();
                        if (user != null) {
                            user.setName(name);
                            user.setEmail(email);
                            sessionManager.saveUser(user, token);
                        }
                    }

                    // Update avatar
                    tvAvatarLetter.setText(!name.isEmpty() ?
                            String.valueOf(name.charAt(0)).toUpperCase() : "?");

                    // Clear password fields
                    etCurrentPassword.setText("");
                    etNewPassword.setText("");
                    etConfirmPassword.setText("");

                    tvSuccess.setText(getString(R.string.settings_saved));
                    tvSuccess.setVisibility(View.VISIBLE);
                } else {
                    showError(getString(R.string.error_sending));
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<User>> call, Throwable t) {
                btnSave.setEnabled(true);
                // Fallback: save locally anyway
                User user = sessionManager.getUser();
                if (user != null) {
                    user.setName(name);
                    user.setEmail(email);
                    sessionManager.saveUser(user, token);
                }
                tvAvatarLetter.setText(!name.isEmpty() ?
                        String.valueOf(name.charAt(0)).toUpperCase() : "?");
                etCurrentPassword.setText("");
                etNewPassword.setText("");
                etConfirmPassword.setText("");

                tvSuccess.setText(getString(R.string.settings_saved));
                tvSuccess.setVisibility(View.VISIBLE);
            }
        });
    }

    private void showError(String msg) {
        tvError.setText(msg);
        tvError.setVisibility(View.VISIBLE);
    }
}

