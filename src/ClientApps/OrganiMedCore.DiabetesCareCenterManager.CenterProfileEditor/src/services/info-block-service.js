const infoBlockConstants = {
    selectorBase: 'center-profile-editor-info',
    classHidden: 'd-none'
};

const infoBlockService = {
    infoBlocks: [],
    hasInfo: false,

    initialize() {
        this.infoBlocks = document.getElementsByClassName(infoBlockConstants.selectorBase);
        this.hasInfo = this.infoBlocks && this.infoBlocks.length;
    },

    hideAll() {
        if (!this.hasInfo) {
            return;
        }

        for (let i = 0; i < this.infoBlocks.length; i++) {
            this.infoBlocks[i].classList.add(infoBlockConstants.classHidden);
        }
    },

    hideAllExpect(stepIndex) {
        if (!this.hasInfo) {
            return;
        }

        this.hideAll();

        let selector = `${infoBlockConstants.selectorBase}-${stepIndex - 1}`;
        let itemsToShow = document.getElementsByClassName(selector);
        if (itemsToShow && itemsToShow.length) {
            for (let i = 0; i < itemsToShow.length; i++) {
                itemsToShow[i].classList.remove(infoBlockConstants.classHidden);
            }
        }
    }
};

export default infoBlockService;