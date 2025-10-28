#include <iostream>
#include <vector>
#include <string>
#include <sstream>
#include <algorithm>
#include <tuple>

using namespace std;

tuple<bool, vector<int>> Solve(const vector<int>& a, int T, int N) {
    vector<vector<bool>> dp(N + 1, vector<bool>(T + 1, false));
    vector<vector<tuple<int, int, int>>> parent(N + 1, vector<tuple<int, int, int>>(T + 1, {-1, -1, -1}));
    dp[0][0] = true;

    for (int i = 1; i <= N; i++) {
        for (int s = 0; s <= T; s++) {
            if (dp[i - 1][s]) {
                dp[i][s] = true;
                parent[i][s] = make_tuple(i - 1, s, 0);
            }
            if (s >= a[i - 1] && dp[i - 1][s - a[i - 1]]) {
                dp[i][s] = true;
                parent[i][s] = make_tuple(i - 1, s - a[i - 1], 1);
            }
        }
    }

    if (!dp[N][T]) return {false, {}};

    vector<int> res;
    int ci = N, cs = T;
    while (ci > 0) {
        auto [pi, ps, take] = parent[ci][cs];
        if (take == 1) res.push_back(ci - 1);
        ci = pi;
        cs = ps;
    }
    sort(res.begin(), res.end());
    return {true, res};
}

int main() {
    string line;
    getline(cin, line);
    stringstream ss(line);
    int N, C;
    ss >> N >> C;

    getline(cin, line);
    ss.clear();
    ss.str(line);
    vector<int> w(N);
    for (int i = 0; i < N; i++) {
        ss >> w[i];
    }

    auto [ok, chosen] = Solve(w, C, N);
    if (!ok) {
        cout << "NO\n";
        return 0;
    }
    cout << "YES\n";
    for (size_t i = 0; i < chosen.size(); i++) {
        if (i > 0) cout << " ";
        cout << chosen[i] + 1;
    }
    cout << "\n";

    return 0;
}
