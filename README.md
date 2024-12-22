# Opt Map Texture Helper

YMToon で利用される opt 系テクスチャの編集をサポートするツール

## 商品構成

* Unity Package(dev.xiphia.optmap-texture-helper-0.1.x-installer.unitypackage)
* ライセンスファイル(LICENSE)
* README (README.md)

## 導入手順

* Unity Package でインストールする場合
    * dev.xiphia.optmap-texture-helper-0.1.x-installer.unitypackage パッケージを Unity プロジェクトにインポートします
    * インストールダイアログが表示されるので Install ボタンを押します
* VPM リポジトリからインストールする場合
    * VCC などで `https://xiphia.github.io/vrc/vpm.json` をリポジトリに追加し、Opt Map Texture Helper をプロジェクトに追加します

## 使用方法

### 右クリックメニューからの使用

1. 分解、再構成したいテクスチャを右クリックしメニューを表示します
    * 分解する場合
        1. チャンネルごとに分解する場合は Opt Map Texture Helper -> Separate Channels を選択します
        2. 分解されたテクスチャが元のテクスチャと同じ場所に出力されます
            * 出力されるテクスチャ名は次のルールに従います
                1. Base opt マップ( `*_opt` ) の場合
                    * `*_opt_rim` : RimLight マスク(R チャンネル)
                    * `*_opt_out` : Outline マスク(G チャンネル)
                    * `*_opt_sss` : SSS マスク(B チャンネル)
                2. Specular opt マップ( `*_spe_opt` ) の場合
                    * `*_spe_opt_nse` : ノイズテクスチャ(R チャンネル)
                    * `*_spe_opt_nse_msk` : ノイズマスク(G チャンネル)
                    * `*_spe_opt_fth` : Feather マスク(B チャンネル)
                3. その他のテクスチャの場合
                    * `*_red` : R チャンネル
                    * `*_grn` : G チャンネル
                    * `*_blu` : B チャンネル
                    * `*_alp` : アルファチャンネル(アルファチャンネルが存在している場合)
    * 再構成する場合
        1. 再構成する場合は Opt Map Texture Helper -> Combine Channels を選択します
            * 上記ルールに沿ったファイル名のテクスチャであればどれを選択しても構いません
        2. 再構成されたテクスチャが R チャンネルに相当するテクスチャとと同じ場所に出力されます
            * 出力されるテクスチャ名は次のルールに従います
                1. Base opt マップ( `*_opt` ) の場合
                    * `*_opt_mod`
                2. Specular opt マップ( `*_spe_opt` ) の場合
                    * `*_spe_opt_mod`
                3. その他のテクスチャの場合
                    * `*_mod`

### GUI からの使用

1. 上部メニューから Tools -> Opt Map Texture Helper を選択します
2. GUI 画面が開くので、出力したいフォーマットを Output Format から選択します
3. テクスチャの分解、再構成に応じて次の操作をします
    * テクスチャの分解を行う場合
        1. Texture Separator の Texture に分解したいテクスチャを指定します
        2. Separete ボタンを押すと分解されたテクスチャが元のテクスチャと同じ場所に出力されます
            * 出力されるテクスチャ名は次のルールに従います
                1. Base opt マップ( `*_opt` ) の場合
                    * `*_opt_rim` : RimLight マスク(R チャンネル)
                    * `*_opt_out` : Outline マスク(G チャンネル)
                    * `*_opt_sss` : SSS マスク(B チャンネル)
                2. Specular opt マップ( `*_spe_opt` ) の場合
                    * `*_spe_opt_nse` : ノイズテクスチャ(R チャンネル)
                    * `*_spe_opt_nse_msk` : ノイズマスク(G チャンネル)
                    * `*_spe_opt_fth` : Feather マスク(B チャンネル)
                3. その他のテクスチャの場合
                    * `*_red` : R チャンネル
                    * `*_grn` : G チャンネル
                    * `*_blu` : B チャンネル
                    * `*_alp` : アルファチャンネル(アルファチャンネルが存在している場合)
    * テクスチャの再構成を行う場合
        1. Texture Combiner の各 Texture に再構成したいテクスチャを指定します
            * その他のテクスチャでアルファチャンネルを利用する場合、Use Alpha Channel にチェックを入れます
        2. Combine ボタンを押すと再構成されたテクスチャが R チャンネルに相当するテクスチャとと同じ場所に出力されます
            * 出力されるテクスチャ名は次のルールに従います
                1. Base opt マップ( `*_opt` ) の場合
                    * `*_opt_mod`
                2. Specular opt マップ( `*_spe_opt` ) の場合
                    * `*_spe_opt_mod`
                3. その他のテクスチャの場合
                    * `*_mod`

## ライセンス

本製品は MIT ライセンスで提供されます。詳細は同梱の LICENSE ファイルをご確認ください。

## 更新履歴

* 2024/12/22 Ver 0.1.1 公開開始
