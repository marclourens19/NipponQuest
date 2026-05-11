using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NipponQuest.Models;

namespace NipponQuest.Data
{
    public static class DbInitializer
    {
        public static void SeedKanaBlitzData(ApplicationDbContext context)
        {
            var existingKeys = new HashSet<string>(
                context.KanaWords
                    .AsNoTracking()
                    .Select(w => w.WordKana + "|" + w.DifficultyLevel + "|" + w.Alphabet)
                    .ToList(),
                StringComparer.Ordinal
            );

            var words = new List<KanaWord>();

            // ╔════════════════════════════════════════════════════════════════╗
            // ║  EASY  —  Single Characters                                    ║
            // ╚════════════════════════════════════════════════════════════════╝
            var easy = new List<(string kana, string romaji, string alphabet)>
            {
                ("あ","A","hiragana"),("い","I","hiragana"),("う","U","hiragana"),("え","E","hiragana"),("お","O","hiragana"),
                ("か","KA","hiragana"),("き","KI","hiragana"),("く","KU","hiragana"),("け","KE","hiragana"),("こ","KO","hiragana"),
                ("さ","SA","hiragana"),("し","SHI","hiragana"),("す","SU","hiragana"),("せ","SE","hiragana"),("そ","SO","hiragana"),
                ("た","TA","hiragana"),("ち","CHI","hiragana"),("つ","TSU","hiragana"),("て","TE","hiragana"),("と","TO","hiragana"),
                ("な","NA","hiragana"),("に","NI","hiragana"),("ぬ","NU","hiragana"),("ね","NE","hiragana"),("の","NO","hiragana"),
                ("は","HA","hiragana"),("ひ","HI","hiragana"),("ふ","FU","hiragana"),("へ","HE","hiragana"),("ほ","HO","hiragana"),
                ("ま","MA","hiragana"),("み","MI","hiragana"),("む","MU","hiragana"),("め","ME","hiragana"),("も","MO","hiragana"),
                ("や","YA","hiragana"),("ゆ","YU","hiragana"),("よ","YO","hiragana"),
                ("ら","RA","hiragana"),("り","RI","hiragana"),("る","RU","hiragana"),("れ","RE","hiragana"),("ろ","RO","hiragana"),
                ("わ","WA","hiragana"),("を","WO","hiragana"),("ん","N","hiragana"),
                ("ア","A","katakana"),("イ","I","katakana"),("ウ","U","katakana"),("エ","E","katakana"),("オ","O","katakana"),
                ("カ","KA","katakana"),("キ","KI","katakana"),("ク","KU","katakana"),("ケ","KE","katakana"),("コ","KO","katakana"),
                ("サ","SA","katakana"),("シ","SHI","katakana"),("ス","SU","katakana"),("セ","SE","katakana"),("ソ","SO","katakana"),
                ("タ","TA","katakana"),("チ","CHI","katakana"),("ツ","TSU","katakana"),("テ","TE","katakana"),("ト","TO","katakana"),
                ("ナ","NA","katakana"),("ニ","NI","katakana"),("ヌ","NU","katakana"),("ネ","NE","katakana"),("ノ","NO","katakana"),
                ("ハ","HA","katakana"),("ヒ","HI","katakana"),("フ","FU","katakana"),("ヘ","HE","katakana"),("ホ","HO","katakana"),
                ("マ","MA","katakana"),("ミ","MI","katakana"),("ム","MU","katakana"),("メ","ME","katakana"),("モ","MO","katakana"),
                ("ヤ","YA","katakana"),("ユ","YU","katakana"),("ヨ","YO","katakana"),
                ("ラ","RA","katakana"),("リ","RI","katakana"),("ル","RU","katakana"),("レ","RE","katakana"),("ロ","RO","katakana"),
                ("ワ","WA","katakana"),("ヲ","WO","katakana"),("ン","N","katakana"),
                ("が","GA","dakuten"),("ぎ","GI","dakuten"),("ぐ","GU","dakuten"),("げ","GE","dakuten"),("ご","GO","dakuten"),
                ("ざ","ZA","dakuten"),("じ","JI","dakuten"),("ず","ZU","dakuten"),("ぜ","ZE","dakuten"),("ぞ","ZO","dakuten"),
                ("だ","DA","dakuten"),("ぢ","DI","dakuten"),("づ","DU","dakuten"),("で","DE","dakuten"),("ど","DO","dakuten"),
                ("ば","BA","dakuten"),("び","BI","dakuten"),("ぶ","BU","dakuten"),("べ","BE","dakuten"),("ぼ","BO","dakuten"),
                ("ぱ","PA","dakuten"),("ぴ","PI","dakuten"),("ぷ","PU","dakuten"),("ぺ","PE","dakuten"),("ぽ","PO","dakuten"),
                ("ゃ","small ya","dakuten"),("ゅ","small yu","dakuten"),("ょ","small yo","dakuten"),("っ","small tsu","dakuten"),("ー","long vowel","dakuten"),
                ("ガ","GA","dakuten"),("ギ","GI","dakuten"),("グ","GU","dakuten"),("ゲ","GE","dakuten"),("ゴ","GO","dakuten"),
                ("ザ","ZA","dakuten"),("ジ","JI","dakuten"),("ズ","ZU","dakuten"),("ゼ","ZE","dakuten"),("ゾ","ZO","dakuten"),
                ("ダ","DA","dakuten"),("ヂ","DI","dakuten"),("ヅ","DU","dakuten"),("デ","DE","dakuten"),("ド","DO","dakuten"),
                ("バ","BA","dakuten"),("ビ","BI","dakuten"),("ブ","BU","dakuten"),("ベ","BE","dakuten"),("ボ","BO","dakuten"),
                ("パ","PA","dakuten"),("ピ","PI","dakuten"),("プ","PU","dakuten"),("ペ","PE","dakuten"),("ポ","PO","dakuten"),
                ("ャ","small YA","dakuten"),("ュ","small YU","dakuten"),("ョ","small YO","dakuten"),("ッ","small TSU","dakuten"),
            };

            foreach (var e in easy)
            {
                var key = e.kana + "|easy|" + e.alphabet;
                if (existingKeys.Contains(key)) continue;
                existingKeys.Add(key);
                words.Add(new KanaWord
                {
                    WordKana = e.kana,
                    WordRomaji = e.romaji,
                    MeaningEnglish = "Single Character",
                    Alphabet = e.alphabet,
                    DifficultyLevel = "easy",
                    CategoryTag = e.alphabet == "dakuten" ? "Dakuten / Special" : "Core Syllabary"
                });
            }

            // ╔════════════════════════════════════════════════════════════════╗
            // ║  NORMAL  —  Common Words                                       ║
            // ╚════════════════════════════════════════════════════════════════╝
            var normal = new List<(string word, string missing, string meaning, string romaji, string alphabet, string tag)>
            {
                ("ねこ","ね","Cat","neko","hiragana","Animals"), ("いぬ","い","Dog","inu","hiragana","Animals"),
                ("とり","と","Bird","tori","hiragana","Animals"), ("さかな","か","Fish","sakana","hiragana","Animals"),
                ("うま","う","Horse","uma","hiragana","Animals"), ("うし","う","Cow","ushi","hiragana","Animals"),
                ("くま","く","Bear","kuma","hiragana","Animals"), ("さる","さ","Monkey","saru","hiragana","Animals"),
                ("やま","や","Mountain","yama","hiragana","Nature"), ("かわ","か","River","kawa","hiragana","Nature"),
                ("みず","み","Water","mizu","hiragana","Nature"), ("そら","そ","Sky","sora","hiragana","Nature"),
                ("はな","は","Flower","hana","hiragana","Nature"), ("つき","つ","Moon","tsuki","hiragana","Nature"),
                ("ほし","ほ","Star","hoshi","hiragana","Nature"), ("うみ","う","Sea","umi","hiragana","Nature"),
                ("もり","も","Forest","mori","hiragana","Nature"), ("いし","い","Stone","ishi","hiragana","Nature"),
                ("くも","く","Cloud","kumo","hiragana","Nature"), ("ゆき","ゆ","Snow","yuki","hiragana","Nature"),
                ("あめ","あ","Rain","ame","hiragana","Nature"), ("かぜ","か","Wind","kaze","hiragana","Nature"),
                ("いえ","い","House","ie","hiragana","Places"), ("みせ","せ","Shop","mise","hiragana","Places"),
                ("えき","え","Station","eki","hiragana","Places"), ("にわ","に","Garden","niwa","hiragana","Places"),
                ("みち","み","Road","michi","hiragana","Places"), ("まち","ま","Town","machi","hiragana","Places"),
                ("むら","む","Village","mura","hiragana","Places"), ("くに","く","Country","kuni","hiragana","Places"),
                ("へや","へ","Room","heya","hiragana","Places"), ("がっこう","が","School","gakkou","hiragana","Places"),
                ("としょかん","と","Library","toshokan","hiragana","Places"), ("びょういん","び","Hospital","byouin","hiragana","Places"),
                ("つくえ","つ","Desk","tsukue","hiragana","Objects"), ("いす","い","Chair","isu","hiragana","Objects"),
                ("ほん","ほ","Book","hon","hiragana","Objects"), ("かばん","ば","Bag","kaban","hiragana","Objects"),
                ("とけい","と","Watch","tokei","hiragana","Objects"), ("かさ","か","Umbrella","kasa","hiragana","Objects"),
                ("くつ","く","Shoes","kutsu","hiragana","Objects"), ("ふく","ふ","Clothes","fuku","hiragana","Objects"),
                ("はこ","は","Box","hako","hiragana","Objects"), ("かみ","か","Paper","kami","hiragana","Objects"),
                ("ともだち","と","Friend","tomodachi","hiragana","People"), ("せんせい","せ","Teacher","sensei","hiragana","People"),
                ("かぞく","か","Family","kazoku","hiragana","People"), ("ひと","ひ","Person","hito","hiragana","People"),
                ("おとこ","お","Man","otoko","hiragana","People"), ("おんな","お","Woman","onna","hiragana","People"),
                ("ちち","ち","Father","chichi","hiragana","People"), ("はは","は","Mother","haha","hiragana","People"),
                ("たべる","た","To Eat","taberu","hiragana","Verbs"), ("のむ","の","To Drink","nomu","hiragana","Verbs"),
                ("みる","み","To See","miru","hiragana","Verbs"), ("いく","い","To Go","iku","hiragana","Verbs"),
                ("くる","く","To Come","kuru","hiragana","Verbs"), ("はなす","は","To Speak","hanasu","hiragana","Verbs"),
                ("かう","か","To Buy","kau","hiragana","Verbs"), ("つくる","つ","To Make","tsukuru","hiragana","Verbs"),
                ("かく","か","To Write","kaku","hiragana","Verbs"), ("よむ","よ","To Read","yomu","hiragana","Verbs"),
                ("きく","き","To Listen","kiku","hiragana","Verbs"), ("はしる","は","To Run","hashiru","hiragana","Verbs"),
                ("あるく","あ","To Walk","aruku","hiragana","Verbs"), ("ねる","ね","To Sleep","neru","hiragana","Verbs"),
                ("おきる","お","To Wake Up","okiru","hiragana","Verbs"), ("あさ","あ","Morning","asa","hiragana","Time"),
                ("ひる","ひ","Noon","hiru","hiragana","Time"), ("よる","よ","Night","yoru","hiragana","Time"),
                ("はる","は","Spring","haru","hiragana","Time"), ("なつ","な","Summer","natsu","hiragana","Time"),
                ("あき","あ","Autumn","aki","hiragana","Time"), ("ふゆ","ふ","Winter","fuyu","hiragana","Time"),
                ("きのう","き","Yesterday","kinou","hiragana","Time"), ("あした","あ","Tomorrow","ashita","hiragana","Time"),
                ("いま","い","Now","ima","hiragana","Time"), ("きょう","き","Today","kyou","hiragana","Time"),
                ("あか","あ","Red","aka","hiragana","Colors"), ("あお","あ","Blue","ao","hiragana","Colors"),
                ("しろ","し","White","shiro","hiragana","Colors"), ("くろ","く","Black","kuro","hiragana","Colors"),
                ("きいろ","き","Yellow","kiiro","hiragana","Colors"), ("みどり","み","Green","midori","hiragana","Colors"),
                ("あつい","あ","Hot","atsui","hiragana","Adjectives"), ("さむい","さ","Cold","samui","hiragana","Adjectives"),
                ("たかい","た","Tall/Expensive","takai","hiragana","Adjectives"), ("やすい","や","Cheap","yasui","hiragana","Adjectives"),
                ("ちいさい","ち","Small","chiisai","hiragana","Adjectives"), ("おおきい","お","Big","ookii","hiragana","Adjectives"),
                ("ながい","な","Long","nagai","hiragana","Adjectives"), ("みじかい","み","Short","mijikai","hiragana","Adjectives"),
                ("はやい","は","Fast","hayai","hiragana","Adjectives"), ("おそい","お","Slow","osoi","hiragana","Adjectives"),
                ("あたらしい","あ","New","atarashii","hiragana","Adjectives"), ("ふるい","ふ","Old","furui","hiragana","Adjectives"),
                ("おいしい","お","Delicious","oishii","hiragana","Adjectives"), ("たのしい","た","Fun","tanoshii","hiragana","Adjectives"),
                ("つまらない","つ","Boring","tsumaranai","hiragana","Adjectives"), ("おもしろい","お","Interesting","omoshiroi","hiragana","Adjectives"),
                ("むずかしい","む","Difficult","muzukashii","hiragana","Adjectives"), ("やさしい","や","Kind/Easy","yasashii","hiragana","Adjectives"),
                ("つよい","つ","Strong","tsuyoi","hiragana","Adjectives"), ("よわい","よ","Weak","yowai","hiragana","Adjectives"),
                ("あかるい","あ","Bright","akarui","hiragana","Adjectives"), ("くらい","く","Dark","kurai","hiragana","Adjectives"),
                ("あまい","あ","Sweet","amai","hiragana","Adjectives"), ("からい","か","Spicy","karai","hiragana","Adjectives"),
                ("うれしい","う","Happy","ureshii","hiragana","Adjectives"), ("かなしい","か","Sad","kanashii","hiragana","Adjectives"),
                ("こわい","こ","Scary","kowai","hiragana","Adjectives"), ("おちゃ","お","Tea","ocha","hiragana","Food"),
                ("ごはん","ご","Rice","gohan","hiragana","Food"), ("パン","パ","Bread","pan","hiragana","Food"),
                ("たまご","た","Egg","tamago","hiragana","Food"), ("にく","に","Meat","niku","hiragana","Food"),
                ("さかな","さ","Fish","sakana","hiragana","Food"), ("やさい","や","Vegetable","yasai","hiragana","Food"),
                ("くだもの","く","Fruit","kudamono","hiragana","Food"), ("りんご","り","Apple","ringo","hiragana","Food"),
                ("バナナ","バ","Banana","banana","hiragana","Food"), ("ぶどう","ぶ","Grapes","budou","hiragana","Food"),
                ("いちご","い","Strawberry","ichigo","hiragana","Food"), ("ラーメン","ラ","Ramen","raamen","hiragana","Food"),
                ("うどん","う","Udon","udon","hiragana","Food"), ("そば","そ","Soba","soba","hiragana","Food"),
                ("すし","す","Sushi","sushi","hiragana","Food"), ("てんぷら","て","Tempura","tenpura","hiragana","Food"),
                ("カレー","カ","Curry","karee","hiragana","Food"), ("ハンバーガー","ハ","Hamburger","hanbaagaa","hiragana","Food"),
                ("ピザ","ピ","Pizza","piza","hiragana","Food"), ("サラダ","サ","Salad","sarada","hiragana","Food"),
                ("スープ","ス","Soup","suupu","hiragana","Food"), ("アイスクリーム","ア","Ice Cream","aisukuriimu","hiragana","Food"),
                ("ケーキ","ケ","Cake","keeki","hiragana","Food"), ("チョコレート","チ","Chocolate","chokoreeto","hiragana","Food"),
                ("あたま","あ","Head","atama","hiragana","Body"), ("かお","か","Face","kao","hiragana","Body"),
                ("かみ","か","Hair","kami","hiragana","Body"), ("め","め","Eye","me","hiragana","Body"),
                ("みみ","み","Ear","mimi","hiragana","Body"), ("くち","く","Mouth","kuchi","hiragana","Body"),
                ("は","は","Tooth","ha","hiragana","Body"), ("した","し","Tongue","shita","hiragana","Body"),
                ("くび","く","Neck","kubi","hiragana","Body"), ("うで","う","Arm","ude","hiragana","Body"),
                ("て","て","Hand","te","hiragana","Body"), ("ゆび","ゆ","Finger","yubi","hiragana","Body"),
                ("あし","あ","Leg/Foot","ashi","hiragana","Body"), ("ひざ","ひ","Knee","hiza","hiragana","Body"),
                ("おかね","お","Money","okane","hiragana","Daily Life"), ("カメラ","カ","Camera","kamera","katakana","Technology"),
                ("テレビ","テ","Television","terebi","katakana","Technology"), ("ラジオ","ラ","Radio","rajio","katakana","Technology"),
                ("パソコン","パ","Computer","pasokon","katakana","Technology"), ("スマートフォン","ス","Smartphone","sumaatofon","katakana","Technology"),
                ("インターネット","イ","Internet","intaanetto","katakana","Technology"), ("メール","メ","Email","meeru","katakana","Technology"),
                ("ゲーム","ゲ","Game","geemu","katakana","Technology"), ("アメリカ","ア","America","amerika","katakana","Countries"),
                ("カナダ","カ","Canada","kanada","katakana","Countries"), ("イギリス","イ","UK","igirisu","katakana","Countries"),
                ("フランス","フ","France","furansu","katakana","Countries"), ("ドイツ","ド","Germany","doitsu","katakana","Countries"),
                ("イタリア","イ","Italy","itaria","katakana","Countries"), ("スペイン","ス","Spain","supein","katakana","Countries"),
                ("ロシア","ロ","Russia","roshia","katakana","Countries"), ("中国","チ","China","chuugoku","katakana","Countries"),
                ("韓国","カ","Korea","kankoku","katakana","Countries"), ("インド","イ","India","indo","katakana","Countries"),
                ("サッカー","サ","Soccer","sakkaa","katakana","Sports"), ("野球","ヤ","Baseball","yakyuu","katakana","Sports"),
                ("テニス","テ","Tennis","tenisu","katakana","Sports"), ("ゴルフ","ゴ","Golf","gorufu","katakana","Sports"),
                ("ピアノ","ピ","Piano","piano","katakana","Music"), ("ギター","ギ","Guitar","gitaa","katakana","Music"),
                ("でんき","で","Electricity","denki","dakuten","Daily Life"), ("でんわ","で","Telephone","denwa","dakuten","Daily Life"),
                ("でんしゃ","で","Train","densha","dakuten","Transport"), ("ぎんこう","ぎ","Bank","ginkou","dakuten","Places"),
                ("びょういん","び","Hospital","byouin","dakuten","Places"), ("だいがく","だ","University","daigaku","dakuten","Places"),
                ("としょかん","と","Library","toshokan","dakuten","Places"), ("はくぶつかん","は","Museum","hakubutsukan","dakuten","Places"),
                ("どうぶつえん","ど","Zoo","doubutsuen","dakuten","Places"), ("こうえん","こ","Park","kouen","dakuten","Places"),
                ("じんじゃ","じ","Shrine","jinja","dakuten","Places"), ("おてら","お","Temple","otera","dakuten","Places"),
            };

            foreach (var n in normal)
            {
                var key = n.word + "|normal|" + n.alphabet;
                if (existingKeys.Contains(key)) continue;
                existingKeys.Add(key);
                words.Add(new KanaWord
                {
                    WordKana = n.word,
                    WordRomaji = n.romaji,
                    MeaningEnglish = n.meaning,
                    Alphabet = n.alphabet,
                    DifficultyLevel = "normal",
                    MissingKana = n.missing,
                    CategoryTag = n.tag
                });
            }

            // ╔════════════════════════════════════════════════════════════════╗
            // ║  HARD  —  Yōon, Sokuon & Compounds                             ║
            // ╚════════════════════════════════════════════════════════════════╝
            var hard = new List<(string word, string missing, string meaning, string romaji, string alphabet, string tag)>
            {
                ("きょうしつ","きょ","Classroom","kyoushitsu","hiragana","Education"), ("しゃしん","しゃ","Photograph","shashin","hiragana","Objects"),
                ("じゅぎょう","じゅ","Class","jugyou","hiragana","Education"), ("りょうり","りょ","Cooking","ryouri","hiragana","Food"),
                ("りゅうがく","りゅ","Studying Abroad","ryuugaku","hiragana","Education"), ("やきゅう","きゅ","Baseball","yakyuu","hiragana","Sports"),
                ("しゅくだい","しゅ","Homework","shukudai","hiragana","Education"), ("にゅうがく","にゅ","Entrance","nyuugaku","hiragana","Education"),
                ("おもちゃ","ちゃ","Toy","omocha","hiragana","Objects"), ("こうちょう","ちょ","Principal","kouchou","hiragana","People"),
                ("こうじょう","じょ","Factory","koujou","hiragana","Places"), ("しゅみ","しゅ","Hobby","shumi","hiragana","Daily Life"),
                ("ひゃく","ひゃ","Hundred","hyaku","hiragana","Numbers"), ("いっしょ","しょ","Together","issho","hiragana","Daily Life"),
                ("けっこん","けっ","Marriage","kekkon","hiragana","Events"), ("ちょっと","ちょ","A little","chotto","hiragana","Daily Life"),
                ("がっこう","が","School","gakkou","hiragana","Places"), ("ざっし","ざ","Magazine","zasshi","hiragana","Objects"),
                ("けっか","け","Result","kekka","hiragana","Concepts"), ("せっけん","せ","Soap","sekken","hiragana","Objects"),
                ("がっき","が","Instrument","gakki","hiragana","Objects"), ("にっき","に","Diary","nikki","hiragana","Objects"),
                ("きっぷ","き","Ticket","kippu","hiragana","Objects"), ("コンピュータ","ピュ","Computer","konpyuuta","katakana","Technology"),
                ("チョコレート","チョ","Chocolate","chokoreeto","katakana","Food"), ("スマートフォン","フォ","Smartphone","sumaatofon","katakana","Technology"),
                ("インターネット","ター","Internet","intaanetto","katakana","Technology"), ("ハンバーガー","バー","Hamburger","hanbaagaa","katakana","Food"),
                ("アイスクリーム","クリ","Ice Cream","aisukuriimu","katakana","Food"), ("ショッピング","ショ","Shopping","shoppingu","katakana","Activities"),
                ("プレゼント","プレ","Present","purezento","katakana","Objects"), ("ファッション","ファ","Fashion","fasshon","katakana","Daily Life"),
                ("シャワー","シャ","Shower","shawaa","katakana","Daily Life"), ("コンビニ","コン","Convenience Store","konbini","katakana","Places"),
                ("スーパー","スー","Supermarket","suupaa","katakana","Places"), ("マンション","マン","Apartment","manshon","katakana","Places"),
                ("レストラン","レ","Restaurant","resutoran","katakana","Places"), ("アプリケーション","ショ","Application","apurikeeshon","katakana","Technology"),
                ("ダウンロード","ロー","Download","daunroodo","katakana","Technology"), ("アップロード","アッ","Upload","appuroodo","katakana","Technology"),
                ("プログラミング","グラ","Programming","puroguramingu","katakana","Technology"), ("クラウド","クラ","Cloud","kuraudo","katakana","Technology"),
                ("バックアップ","バッ","Backup","bakkuappu","katakana","Technology"), ("アレルギー","ルギ","Allergy","arerugii","katakana","Health"),
                ("ワクチン","ワク","Vaccine","wakuchin","katakana","Health"), ("フィットネス","フィ","Fitness","fittonesu","katakana","Health"),
                ("リハビリ","ハビ","Rehabilitation","rihabiri","katakana","Health"), ("ぎゅうにゅう","ぎゅ","Milk","gyuunyuu","dakuten","Food"),
                ("ぎゅうにく","ぎゅ","Beef","gyuuniku","dakuten","Food"), ("びょういん","びょ","Hospital","byouin","dakuten","Places"),
                ("びじゅつかん","じゅ","Art Museum","bijutsukan","dakuten","Places"), ("ぎじゅつ","ぎ","Technology","gijutsu","dakuten","Education"),
                ("ばんごはん","ば","Dinner","bangohan","dakuten","Food"), ("ぶんがく","ぶ","Literature","bungaku","dakuten","Education"),
                ("どようび","ど","Saturday","doyoubi","dakuten","Time"), ("にちようび","び","Sunday","nichiyoubi","dakuten","Time"),
                ("げつようび","げ","Monday","getsuyoubi","dakuten","Time"), ("じょうほう","じょ","Information","jouhou","dakuten","Daily Life"),
                ("しょうがっこう","がっ","Elementary School","shougakkou","dakuten","Places"), ("ぎょうじ","ぎょ","Event","gyouji","dakuten","Events"),
                ("じゅうしょ","じゅ","Address","juusho","dakuten","Daily Life"), ("おばあさん","ば","Grandmother","obaasan","dakuten","People"),
                ("おじいさん","じ","Grandfather","ojiisan","dakuten","People"), ("ぶんぼうぐ","ぶ","Stationery","bunbougu","dakuten","Objects"),
                ("ビザ","ビ","Visa","biza","dakuten","Daily Life"), ("パスポート","パ","Passport","pasupooto","dakuten","Daily Life"),
                ("バッテリー","バッ","Battery","batterii","dakuten","Technology"), ("ゲームセンター","セン","Arcade","geemusentaa","dakuten","Places"),
                ("ボランティア","ボ","Volunteer","borantia","dakuten","Activities"), ("バランス","バ","Balance","baransu","dakuten","Concepts"),
                ("ゴール","ゴ","Goal","gooru","dakuten","Sports"), ("ポジション","ジョ","Position","pojishon","dakuten","Sports"),
                ("ビジネス","ビ","Business","bijinesu","dakuten","Daily Life"), ("ブログ","ブ","Blog","burogu","dakuten","Technology"),
                ("グループ","グ","Group","guruupu","dakuten","Daily Life"), ("デジタル","デ","Digital","dejitaru","dakuten","Technology"),
                ("ビデオ","ビ","Video","bideo","dakuten","Technology"), ("システム","テ","System","shisutemu","dakuten","Technology"),
                ("サービス","ビ","Service","saabisu","dakuten","Concepts"), ("サポート","ポ","Support","sapooto","dakuten","Concepts"),
                ("プロジェクト","ジェ","Project","purojekuto","dakuten","Concepts"), ("プログラム","グラ","Program","puroguramu","dakuten","Technology"),
            };

            foreach (var h in hard)
            {
                var key = h.word + "|hard|" + h.alphabet;
                if (existingKeys.Contains(key)) continue;
                existingKeys.Add(key);
                words.Add(new KanaWord
                {
                    WordKana = h.word,
                    WordRomaji = h.romaji,
                    MeaningEnglish = h.meaning,
                    Alphabet = h.alphabet,
                    DifficultyLevel = "hard",
                    MissingKana = h.missing,
                    CategoryTag = h.tag
                });
            }

            // ╔════════════════════════════════════════════════════════════════╗
            // ║  INSANITY  —  Kanji + Furigana                                 ║
            // ╚════════════════════════════════════════════════════════════════╝
            var insane = new List<(string word, string missing, string meaning, string romaji, string alphabet, string tag)>
            {
                ("学校 (がっこう)","がっ","School","gakkou","hiragana","Places"), ("先生 (せんせい)","せん","Teacher","sensei","hiragana","People"),
                ("学生 (がくせい)","がく","Student","gakusei","hiragana","People"), ("家族 (かぞく)","か","Family","kazoku","hiragana","People"),
                ("名前 (なまえ)","なま","Name","namae","hiragana","People"), ("時間 (じかん)","じ","Time","jikan","hiragana","Daily Life"),
                ("天気 (てんき)","てん","Weather","tenki","hiragana","Nature"), ("電車 (でんしゃ)","でん","Train","densha","hiragana","Transport"),
                ("会社 (かいしゃ)","かい","Company","kaisha","hiragana","Places"), ("病院 (びょういん)","びょう","Hospital","byouin","hiragana","Places"),
                ("旅行 (りょこう)","りょ","Travel","ryokou","hiragana","Activities"), ("勉強 (べんきょう)","べん","Study","benkyou","hiragana","Education"),
                ("結婚 (けっこん)","けっ","Marriage","kekkon","hiragana","Events"), ("友達 (ともだち)","とも","Friend","tomodachi","hiragana","People"),
                ("猫 (ねこ)","ね","Cat","neko","hiragana","Animals"), ("犬 (いぬ)","い","Dog","inu","hiragana","Animals"),
                ("山 (やま)","や","Mountain","yama","hiragana","Nature"), ("川 (かわ)","か","River","kawa","hiragana","Nature"),
                ("花 (はな)","は","Flower","hana","hiragana","Nature"), ("月 (つき)","つ","Moon","tsuki","hiragana","Nature"),
                ("海 (うみ)","う","Sea","umi","hiragana","Nature"), ("心 (こころ)","こ","Heart","kokoro","hiragana","Abstract"),
                ("夢 (ゆめ)","ゆ","Dream","yume","hiragana","Abstract"), ("力 (ちから)","ち","Power","chikara","hiragana","Abstract"),
                ("道 (みち)","み","Road","michi","hiragana","Places"), ("水 (みず)","み","Water","mizu","hiragana","Nature"),
                ("風 (かぜ)","か","Wind","kaze","hiragana","Nature"), ("雨 (あめ)","あ","Rain","ame","hiragana","Nature"),
                ("雪 (ゆき)","ゆ","Snow","yuki","hiragana","Nature"), ("火 (ひ)","ひ","Fire","hi","hiragana","Nature"),
                ("木 (き)","き","Tree","ki","hiragana","Nature"), ("石 (いし)","い","Stone","ishi","hiragana","Nature"),
                ("土 (つち)","つ","Soil","tsuchi","hiragana","Nature"), ("命 (いのち)","の","Life","inochi","hiragana","Abstract"),
                ("声 (こえ)","こ","Voice","koe","hiragana","Abstract"), ("色 (いろ)","い","Colour","iro","hiragana","Abstract"),
                ("形 (かたち)","か","Shape","katachi","hiragana","Abstract"), ("光 (ひかり)","ひ","Light","hikari","hiragana","Nature"),
                ("影 (かげ)","か","Shadow","kage","hiragana","Abstract"), ("橋 (はし)","は","Bridge","hashi","hiragana","Places"),
                ("窓 (まど)","ま","Window","mado","hiragana","Objects"), ("鍵 (かぎ)","か","Key","kagi","hiragana","Objects"),
                ("鏡 (かがみ)","が","Mirror","kagami","hiragana","Objects"), ("話 (はなし)","な","Story","hanashi","hiragana","Abstract"),
                ("答え (こたえ)","た","Answer","kotae","hiragana","Education"), ("問題 (もんだい)","もん","Problem","mondai","hiragana","Education"),
                ("練習 (れんしゅう)","しゅ","Practice","renshuu","hiragana","Education"), ("試験 (しけん)","しけ","Exam","shiken","hiragana","Education"),
                ("合格 (ごうかく)","ごう","Pass","goukaku","hiragana","Education"), ("失敗 (しっぱい)","しっ","Failure","shippai","hiragana","Concepts"),
                ("成功 (せいこう)","せい","Success","seikou","hiragana","Concepts"), ("努力 (どりょく)","りょ","Effort","doryoku","hiragana","Concepts"),
                ("勇気 (ゆうき)","ゆ","Courage","yuuki","hiragana","Concepts"), ("自信 (じしん)","じ","Confidence","jishin","hiragana","Concepts"),
                ("幸福 (こうふく)","こう","Happiness","koufuku","hiragana","Emotions"), ("悲しみ (かなしみ)","かな","Sadness","kanashimi","hiragana","Emotions"),
                ("喜び (よろこび)","ろこ","Joy","yorokobi","hiragana","Emotions"), ("怒り (いかり)","い","Anger","ikari","hiragana","Emotions"),
                ("恐れ (おそれ)","お","Fear","osore","hiragana","Emotions"), ("珈琲 (コーヒー)","コー","Coffee","koohii","katakana","Food"),
                ("麦酒 (ビール)","ビー","Beer","biiru","katakana","Food"), ("天麩羅 (テンプラ)","プラ","Tempura","tenpura","katakana","Food"),
                ("寿司 (スシ)","ス","Sushi","sushi","katakana","Food"), ("葡萄 (ブドウ)","ブ","Grapes","budou","katakana","Food"),
                ("拉麺 (ラーメン)","ラー","Ramen","raamen","katakana","Food"), ("檸檬 (レモン)","レ","Lemon","remon","katakana","Food"),
                ("林檎 (リンゴ)","リン","Apple","ringo","katakana","Food"), ("亜米利加 (アメリカ)","メリ","America","amerika","katakana","Countries"),
                ("英国 (イギリス)","イ","Britain","igirisu","katakana","Countries"), ("仏蘭西 (フランス)","フラ","France","furansu","katakana","Countries"),
                ("独逸 (ドイツ)","ド","Germany","doitsu","katakana","Countries"), ("伊太利 (イタリア)","タリ","Italy","itaria","katakana","Countries"),
                ("銀行 (ぎんこう)","ぎん","Bank","ginkou","dakuten","Places"), ("大学 (だいがく)","だい","University","daigaku","dakuten","Places"),
                ("元気 (げんき)","げん","Healthy","genki","dakuten","Adjectives"), ("電気 (でんき)","でん","Electricity","denki","dakuten","Daily Life"),
                ("電話 (でんわ)","でん","Telephone","denwa","dakuten","Technology"), ("動物 (どうぶつ)","ぶつ","Animal","doubutsu","dakuten","Animals"),
                ("病気 (びょうき)","びょ","Sickness","byouki","dakuten","Health"), ("文学 (ぶんがく)","ぶん","Literature","bungaku","dakuten","Education"),
                ("音楽 (おんがく)","がく","Music","ongaku","dakuten","Music"), ("漫画 (まんが)","が","Manga","manga","dakuten","Media"),
                ("健康 (けんこう)","けん","Health","kenkou","dakuten","Health"), ("研究 (けんきゅう)","きゅ","Research","kenkyuu","dakuten","Education"),
                ("経験 (けいけん)","けん","Experience","keiken","dakuten","Concepts"), ("協力 (きょうりょく)","りょ","Cooperation","kyouryoku","dakuten","Concepts"),
                ("子供 (こども)","ど","Child","kodomo","dakuten","People"), ("社会 (しゃかい)","しゃ","Society","shakai","dakuten","Concepts"),
                ("政府 (せいふ)","せい","Government","seifu","dakuten","Concepts"), ("大使館 (たいしかん)","しか","Embassy","taishikan","dakuten","Places"),
                ("図書館 (としょかん)","しょ","Library","toshokan","dakuten","Places"), ("博物館 (はくぶつかん)","ぶつ","Museum","hakubutsukan","dakuten","Places"),
                ("駅 (えき)","え","Station","eki","dakuten","Places"), ("道路 (どうろ)","ど","Road","douro","dakuten","Transport"),
                ("飛行機 (ひこうき)","こう","Airplane","hikouki","dakuten","Transport"), ("新幹線 (しんかんせん)","かん","Bullet Train","shinkansen","dakuten","Transport"),
                ("地下鉄 (ちかてつ)","てつ","Subway","chikatetsu","dakuten","Transport"), ("自転車 (じてんしゃ)","じ","Bicycle","jitensha","dakuten","Transport"),
                ("郵便局 (ゆうびんきょく)","びん","Post Office","yuubinkyoku","dakuten","Places"), ("薬局 (やっきょく)","やっ","Pharmacy","yakkyoku","dakuten","Places"),
                ("映画 (えいが)","が","Movie","eiga","dakuten","Media"), ("演劇 (えんげき)","げ","Theatre","engeki","dakuten","Culture"),
                ("医者 (いしゃ)","いしゃ","Doctor","isha","dakuten","People"), ("弁護士 (べんごし)","べん","Lawyer","bengoshi","dakuten","People"),
                ("消防士 (しょうぼうし)","しょ","Firefighter","shouboushi","dakuten","People"), ("警察官 (けいさつかん)","さつ","Police","keisatsukan","dakuten","People"),
                ("日本語 (にほんご)","ほん","Japanese","nihongo","mixed","Language"), ("新幹線 (しんかんせん)","かん","Bullet Train","shinkansen","mixed","Transport"),
                ("富士山 (ふじさん)","じ","Mount Fuji","fujisan","mixed","Geography"), ("図書館 (としょかん)","しょ","Library","toshokan","mixed","Places"),
                ("郵便局 (ゆうびんきょく)","びん","Post Office","yuubinkyoku","mixed","Places"), ("地下鉄 (ちかてつ)","ちか","Subway","chikatetsu","mixed","Transport"),
                ("大学生 (だいがくせい)","がく","University Student","daigakusei","mixed","People"), ("会社員 (かいしゃいん)","しゃ","Office Worker","kaishain","mixed","People"),
                ("天気予報 (てんきよほう)","よほ","Weather Forecast","tenkiyohou","mixed","Daily Life"), ("飛行機 (ひこうき)","こう","Airplane","hikouki","mixed","Transport"),
                ("美術館 (びじゅつかん)","じゅ","Art Museum","bijutsukan","mixed","Places"), ("結婚式 (けっこんしき)","こん","Wedding","kekkonshiki","mixed","Events"),
                ("一期一会 (いちごいちえ)","ご","Once in a Lifetime","ichigoichie","mixed","Idioms"), ("七転八起 (しちてんはっき)","てん","Perseverance","shichitenhakki","mixed","Idioms"),
                ("一石二鳥 (いっせきにちょう)","ちょ","Two Birds One Stone","issekinichou","mixed","Idioms"), ("十人十色 (じゅうにんといろ)","じゅ","To Each Their Own","juunintoiro","mixed","Idioms"),
                ("月見 (つきみ)","つき","Moon Viewing","tsukimi","mixed","Culture"), ("花見 (はなみ)","はな","Cherry Blossom","hanami","mixed","Culture"),
                ("茶道 (さどう)","さ","Tea Ceremony","sadou","mixed","Culture"), ("書道 (しょどう)","しょ","Calligraphy","shodou","mixed","Culture"),
                ("剣道 (けんどう)","けん","Kendo","kendou","mixed","Sports"), ("柔道 (じゅうどう)","じゅ","Judo","juudou","mixed","Sports"),
                ("空手 (からて)","から","Karate","karate","mixed","Sports"), ("着物 (きもの)","き","Kimono","kimono","mixed","Culture"),
                ("神社 (じんじゃ)","じん","Shrine","jinja","mixed","Places"), ("城 (しろ)","し","Castle","shiro","mixed","Places"),
                ("空港 (くうこう)","くう","Airport","kuukou","mixed","Places"), ("経済 (けいざい)","ざ","Economy","keizai","mixed","Concepts"),
                ("文化 (ぶんか)","ぶん","Culture","bunka","mixed","Culture"), ("歴史 (れきし)","れ","History","rekishi","mixed","Concepts"),
                ("科学 (かがく)","が","Science","kagaku","mixed","Education"), ("哲学 (てつがく)","がく","Philosophy","tetsugaku","mixed","Education"),
                ("心理学 (しんりがく)","しん","Psychology","shinrigaku","mixed","Education"), ("宇宙 (うちゅう)","ちゅ","Universe","uchuu","mixed","Nature"),
                ("地球 (ちきゅう)","きゅ","Earth","chikyuu","mixed","Nature"), ("自然 (しぜん)","ぜん","Nature","shizen","mixed","Nature"),
                ("環境 (かんきょう)","きょ","Environment","kankyou","mixed","Concepts"), ("政治 (せいじ)","じ","Politics","seiji","mixed","Concepts"),
                ("自由 (じゆう)","じ","Freedom","jiyuu","mixed","Concepts"), ("平和 (へいわ)","へい","Peace","heiwa","mixed","Concepts"),
                ("希望 (きぼう)","き","Hope","kibou","mixed","Concepts"), ("真実 (しんじつ)","じつ","Truth","shinjitsu","mixed","Concepts"),
                ("正義 (せいぎ)","せい","Justice","seigi","mixed","Concepts"), ("知識 (ちしき)","しき","Knowledge","chishiki","mixed","Concepts"),
                ("技術 (ぎじゅつ)","じゅ","Technology","gijutsu","mixed","Concepts"), ("革命 (かくめい)","めい","Revolution","kakumei","mixed","History"),
                ("戦争 (せんそう)","せん","War","sensou","mixed","History"), ("世界 (せかい)","せ","World","sekai","mixed","Concepts"),
                ("人類 (じんるい)","じん","Humanity","jinrui","mixed","Concepts"), ("生命 (せいめい)","めい","Life","seimei","mixed","Abstract"),
                ("進化 (しんか)","しん","Evolution","shinka","mixed","Concepts"), ("文明 (ぶんめい)","めい","Civilisation","bunmei","mixed","History"),
                ("伝統 (でんとう)","でん","Tradition","dentou","mixed","Culture"), ("芸術 (げいじゅつ)","じゅ","Art","geijutsu","mixed","Culture"),
                ("建築 (けんちく)","けん","Architecture","kenchiku","mixed","Culture"), ("経営 (けいえい)","けい","Management","keiei","mixed","Concepts"),
                ("投資 (とうし)","とう","Investment","toushi","mixed","Concepts"), ("節約 (せつやく)","せつ","Saving","setsuyaku","mixed","Daily Life"),
                ("目標 (もくひょう)","もく","Goal","mokuhyou","mixed","Concepts"), ("計画 (けいかく)","けい","Plan","keikaku","mixed","Concepts"),
                ("準備 (じゅんび)","じゅ","Preparation","junbi","mixed","Daily Life"), ("確認 (かくにん)","かく","Confirmation","kakunin","mixed","Daily Life"),
                ("連絡 (れんらく)","れん","Contact","renraku","mixed","Daily Life"), ("報告 (ほうこく)","こく","Report","houkoku","mixed","Daily Life"),
                ("相談 (そうだん)","だん","Consultation","soudan","mixed","Daily Life"), ("申し込み (もうしこみ)","もう","Application","moushikomi","mixed","Daily Life"),
                ("手続き (てつづき)","てつ","Procedure","tetsuzuki","mixed","Daily Life"), ("支払い (しはらい)","しは","Payment","shiharai","mixed","Daily Life"),
                ("請求書 (せいきゅうしょ)","きゅ","Invoice","seikyuusho","mixed","Daily Life"), ("領収書 (りょうしゅうしょ)","りょ","Receipt","ryoushusho","mixed","Daily Life"),
                ("起死回生 (きしかいせい)","かい","Miraculous Recovery","kishikaisei","mixed","Idioms"), ("自業自得 (じごうじとく)","じごう","Karma","jigoujitoku","mixed","Idioms"),
                ("四面楚歌 (しめんそか)","しめん","Surrounded","shimensoка","mixed","Idioms"), ("温故知新 (おんこちしん)","ちしん","Learn from Past","onkochishin","mixed","Idioms"),
                ("竜頭蛇尾 (りゅうとうだび)","りゅ","Anti-climax","ryuutoudabi","mixed","Idioms"), ("完全無欠 (かんぜんむけつ)","むけ","Perfect","kanzenmuketsu","mixed","Idioms"),
                ("臨機応変 (りんきおうへん)","りん","Adaptability","rinkiouhen","mixed","Idioms"),
            };

            foreach (var i in insane)
            {
                var key = i.word + "|insanity|" + i.alphabet;
                if (existingKeys.Contains(key)) continue;
                existingKeys.Add(key);
                words.Add(new KanaWord
                {
                    WordKana = i.word,
                    WordRomaji = i.romaji,
                    MeaningEnglish = i.meaning,
                    Alphabet = i.alphabet,
                    DifficultyLevel = "insanity",
                    MissingKana = i.missing,
                    CategoryTag = i.tag
                });
            }

            if (words.Count > 0)
            {
                context.KanaWords.AddRange(words);
                context.SaveChanges();
            }
        }
    }
}
