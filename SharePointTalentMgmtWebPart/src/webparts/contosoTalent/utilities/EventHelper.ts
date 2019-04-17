import styles from '../ContosoTalentWebPart.module.scss';

export class EventHelper {
    /**
     * Add click event listener for class selector.
     * @param classSelector
     * @param callback  Function to do more specific actions.
     * @param stopPropagation True if we need to cancel event propagation.
     */
    public static AddClickEventListener(classSelector: string, callback: Function, stopPropagation: boolean = false): void {
        let elements: HTMLCollectionOf<Element> = document.getElementsByClassName(classSelector);
        for (let i = 0; i < elements.length; i++) {
            elements[i].addEventListener("click", (e: Event) => {
                if (e.target instanceof Element) {
                    callback(e.target);
                    if (stopPropagation) {
                        e.stopPropagation();
                        e.cancelBubble = true;
                    }
                }
            });
        }
    }

    public static RegisterGlobalEventHandlers(): void {
        window.addEventListener("click", (e: Event) => {
            if (e.target instanceof Element && !e.target.matches('.stage-btn')) { this.CloseAllDropdowns(); }
        });

        EventHelper.AddClickEventListener(styles.collapsible, (element: Element) => {
            this.OpenCloseCollapsibleMenus(element);
        });

        EventHelper.AddClickEventListener("position-dropdown", () => {
            this.OpenCloseDropdown(`positionsDropdownContent`);
        }, true);
    }

    public static OpenCloseDropdown(selector: string): void {
        let dropDownElement: HTMLElement = document.getElementById(selector);
        let shown: boolean = dropDownElement.classList.contains(styles.show);
        this.CloseAllDropdowns();
        if(shown){ dropDownElement.classList.remove(styles.show); } else { dropDownElement.classList.add(styles.show); }
    }

    private static OpenCloseCollapsibleMenus(element: Element): void {
        let id: string = element.getAttribute("data-id");

        let relatedObjects: HTMLCollectionOf<Element> = document.getElementsByClassName("cm-data-" + id);
        for (let ro = 0; ro < relatedObjects.length; ro++) {
            let content: HTMLElement = <HTMLElement>relatedObjects[ro];
            content.style.display = content.style.display === "block" ? "none" : "block";
        }

        let chevrons: HTMLCollectionOf<Element> = document.getElementsByClassName("details-" + id);
        for (let ai = 0; ai < chevrons.length; ai++) {
            let content: HTMLElement = <HTMLElement>chevrons[ai];
            if (content.classList.contains(styles.hidden)) content.classList.remove(styles.hidden);
            else content.classList.add(styles.hidden);
        }
    }

    public static CloseAllDropdowns(): void {
        let dropdowns: HTMLCollectionOf<Element> = document.getElementsByClassName(styles.dropdownContent);
        for (let j = 0; j < dropdowns.length; j++) { dropdowns[j].classList.remove(styles.show); }
    }
}