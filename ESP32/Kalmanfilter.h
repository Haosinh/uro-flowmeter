#ifndef KALMAN_FILTER_H
#define KALMAN_FILTER_H

class KalmanFilter {
  public:
    KalmanFilter(float Q, float R, float initial_estimate = 0, float initial_covariance = 1)
      : Q(Q), R(R), x_est(initial_estimate), P_est(initial_covariance) {}

    float filter(float z_meas) {
      // Prediction update
      float x_pred = x_est;      // Predicted state
      float P_pred = P_est + Q;  // Predicted covariance 

      // Measurement update
      float K = P_pred / (P_pred + R);  // Kalman gain
      x_est = x_pred + K * (z_meas - x_pred);  // Updated state estimate
      P_est = (1 - K) * P_pred;               // Updated covariance estimate
      
      return x_est;  // Return the filtered state estimate
    }

  private:
    float Q;      // Process noise covariance
    float R;      // Measurement noise covariance
    float x_est;  // Estimated state
    float P_est;  // Estimated covariance
};

#endif // KALMAN_FILTER_H
