#include <string>
#include <vector>
#include <algorithm>
#include <iostream>
#include <fstream>
#include <map>
#include <set>
#include <regex>

using namespace std;

struct Link
{
    char s; // state
    string inp; // remaining part of the input tape
    string stack; // current stack state
    int index; // possible function value index
    bool term; // whether branch selection can be changed at this step

    Link(char s, string p, string h, bool t) : s(s), inp(p), stack(h), index(-1), term(t) { }
    Link(char s, string p, string h) : s(s), inp(p), stack(h), index(-1) { }
};

struct Fargs
{
    char s; // state
    char p; // symbol from the input tape
    char h; // stack symbol

    Fargs(char s, char p, char h) : s(s), p(p), h(h) { }
};

struct Value
{
    char s;     // state
    string c;   // chain to put on the stack

    Value(char s, string c) : s(s), c(c) { }
};

struct Command
{
    Fargs f;
    vector<Value> values;

    Command(Fargs f, vector<Value> v) : f(f), values(v) { }
};

class Storage
{
private:
    ifstream file;
    set<char> P; // terminal symbols
    set<char> H; // non-terminal symbols
    char s0 = '0', h0 = '|', empty_symbol = '\0';
    vector<Command> commands;
    vector<Link> chain; // chain of configurations of the pushdown automaton obtained during its operation

public:

    Storage(const char* filename) : file(filename)
    {
        if (!file.is_open())
            throw runtime_error("Failed to open file for reading\n");
        string tmpStr;
        int vsize;
        const regex exp("([[:upper:]])>([[:print:]]+)");
        smatch match;
        while (getline(file, tmpStr))
        {
            if (tmpStr.size() == 0)
                continue;
            if (!regex_match(tmpStr, match, exp) || tmpStr[tmpStr.size() - 1] == '|' || tmpStr[2] == '|')
            {
                throw runtime_error("Failed to recognize file content\n");
            }
            else
            {
                H.insert(match[1].str()[0]);
                commands.push_back(Command(Fargs(s0, empty_symbol, match[1].str()[0]), vector<Value>()));
                commands[commands.size() - 1].values.push_back(Value(s0, ""));
                for (int i = 0; i < match[2].str().size(); i++)
                {
                    if (match[2].str()[i] == '|')
                    {
                        if (match[2].str()[i - 1] != '|')
                            commands[commands.size() - 1].values.push_back(Value(s0, ""));
                    }
                    else
                    {
                        P.insert(match[2].str()[i]);
                        vsize = commands[commands.size() - 1].values.size();
                        commands[commands.size() - 1].values[vsize - 1].c.push_back(match[2].str()[i]);
                    }
                }

                for (int i = 0; i < commands[commands.size() - 1].values.size(); i++)
                    reverse(commands[commands.size() - 1].values[i].c.begin(), commands[commands.size() - 1].values[i].c.end());
            }
        }
        for (const auto& c : H)
            P.erase(c);
        for (const auto& c : P)
            commands.push_back(Command(Fargs(s0, c, c), vector<Value>({ Value(s0, "\0") })));
        commands.push_back(Command(Fargs(s0, empty_symbol, h0), vector<Value>({ Value(s0, "\0") })));
    }

    void showInfo()
    {
        cout << "Input alphabet:\nP = {";
        for (const auto& c : P)
            cout << c << ", ";
        cout << "\b\b}\n\n";
        cout << "Stack alphabet:\nZ = {";
        for (const auto& c : H)
            cout << c << ", ";
        for (const auto& c : P)
            cout << c << ", ";
        cout << "h0}\n\n";

        cout << "Command list:\n";
        for (const auto& c : commands)
        {
            cout << "f(s" << c.f.s << ", ";
            if (c.f.p == empty_symbol)
                cout << "lambda";
            else
                cout << c.f.p;
            cout << ", ";
            if (c.f.h == h0)
                cout << "h0";
            else
                cout << c.f.h;
            cout << ") = {";
            for (Value v : c.values)
            {
                cout << "(s" << v.s << ", ";
                if (v.c[0] == empty_symbol)
                    cout << "lambda";
                else
                    cout << v.c;
                cout << "); ";

            }
            cout << "\b\b}\n";
        }
        cout << endl;
    }

    void showChain()
    {
        cout << "\nChain of configurations: \n";
        for (const auto& link : chain)
            cout << "(s" << link.s << ", " << ((link.inp.size() == 0) ? "lambda" : link.inp) << ", h0" << link.stack << ") | – ";
        cout << "(s0, lambda, lambda)" << endl;
    }

    bool push_link(Link current_link)
    {
        int ch_size = chain.size();
        int mag_size, j, i;
        for (i = 0; i < commands.size(); i++) {
            mag_size = chain[ch_size - 1].stack.size();
            if (current_link.inp.size() != 0 && current_link.stack.size() != 0 && current_link.s == commands[i].f.s && (current_link.inp[0] == commands[i].f.p || empty_symbol == commands[i].f.p) && current_link.stack[mag_size - 1] == commands[i].f.h)
            {
                for (j = 0; j < commands[i].values.size(); j++)
                {
                    Link new_link(commands[i].values[j].s, current_link.inp, current_link.stack);
                    if (commands[i].f.p != empty_symbol)
                    {
                        reverse(new_link.inp.begin(), new_link.inp.end());
                        new_link.inp.pop_back();
                        reverse(new_link.inp.begin(), new_link.inp.end());
                    }

                    new_link.stack.pop_back();
                    new_link.stack += commands[i].values[j].c;
                    if (new_link.inp.size() < new_link.stack.size())
                    {
                        // Chain not accepted
                        return false;
                    }
                    else
                    {
                        if (new_link.inp.size() == 0 && new_link.stack.size() == 0)
                        {
                            // Chain accepted
                            chain.push_back(new_link);
                            return true;
                        }
                        else
                        {
                            // Continue to push
                            if (push_link(new_link))
                            {
                                chain.push_back(new_link);
                                return true;
                            }
                        }
                    }
                }
            }
        }
        // No matching command found
        return false;
    }

    bool check_line(const string& str)
    {
        chain.push_back(Link(s0, str, string(""), false));
        chain[0].stack.push_back(commands[0].f.h);

        bool res = push_link(chain[0]);
        if (res)
        {
            cout << "Valid string\n";
            showChain();
        }
        else {
            cout << "Invalid string\n";
        }
        chain.clear();
        return res;
    }

    ~Storage()
    {
        file.close();
    }
};

int main(int argc, char* argv[]) {
    if (argc < 2) {
        std::cerr << "Usage: " << argv[0] << " test.txt\n";
        return EXIT_FAILURE;
    }

    try {
        std::string str;
        Storage strg(argv[1]);
        strg.showInfo();

        // Examples from text file
        std::cout << "Examples from text file:\n";
        strg.check_line("aaaabb");
        strg.check_line("abab");
        strg.check_line("aaabbb");

        // Input from keyboard
        std::cout << "Input from keyboard:\n";
        std::cout << "Enter a string: \n";
        std::getline(std::cin, str);
        strg.check_line(str);

        // Other test cases
        std::cout << "Other test cases:\n";
        strg.check_line("aabbb");
        strg.check_line("ababab");
        strg.check_line("aa");
    }
    catch (const std::exception& err) {
        std::cerr << err.what() << std::endl;
        return EXIT_FAILURE;
    }

    return 0;
}



