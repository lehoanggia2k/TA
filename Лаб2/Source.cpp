#include <iostream>
#include <fstream>
#include <string>
#include <map>
#include <vector>
#include <set>
using namespace std;

map<pair<string, char>, string> d_table;

void parse_automate(const string& file_name)
{
    ifstream file(file_name);
    if (!file.is_open()) {
        cerr << "Error: Could not open file " << file_name << "\n";
        exit(1);
    }

    string str;
    char c;
    string q, f;

    while (getline(file, str))
    {
        q = str.substr(0, str.find(',')); // Extract the current state
        c = str[str.find(',') + 1]; // Extract the input character
        f = str.substr(str.find('=') + 1); // Extract the resulting state
        d_table[make_pair(q, c)] = f; // Add the transition to d_table
    }
}

bool parse_str(const string& str)
{
    string Qcur = "q0"; // Start from the initial state
    size_t len = str.size();
    for (size_t i = 0; i < len; i++)
    {
        auto tmp_pair = make_pair(Qcur, str[i]);
        if (d_table.find(tmp_pair) == d_table.end())
            return false; // If transition not found, return false
        Qcur = d_table[tmp_pair]; // Update current state
    }
    if (Qcur == "q3") // Check if final state is q3
        return true;
    else
        return false;
}

int main()
{
    parse_automate("test.txt"); // Parse the NFA

    // Example strings to test
    vector<string> test_strings = { "acc", "aab", "aca", "abc", "acac", "abac" };

    // Test each string and print result
    for (const auto& str : test_strings)
    {
        bool ret = parse_str(str);
        cout << str << ": " << (ret ? "Valid string" : "Not a valid string") << "\n";
    }
    system("pause"); // Pause the console
    return 0;
}
