import { Candidate } from "../Candidate";
import { Position } from "../Position";
import styles from '../ContosoTalentWebPart.module.scss';

const APPLIED: string = "Applied";
const SCREENING: string = "Screening";
const INTERVIEWING: string = "Interviewing";
const OFFERED: string = "Offered";

export class HeaderBuilder {    
    /**
     * Build header with template.
     */
    public static BuildHeader(candidates: Candidate[], activeStyle: string, offlineMode: boolean): string {
        let applied: Candidate[] = [];
        let interviewing: Candidate[] = [];
        let screening: Candidate[] = [];
        let offered: Candidate[] = [];

        candidates.forEach((c) => {
            if (c.stage === APPLIED) { applied.push(c); }
            else if (c.stage === INTERVIEWING) { interviewing.push(c); }
            else if (c.stage === SCREENING) { screening.push(c); }
            else if (c.stage === OFFERED) { offered.push(c); }
        });

        let offlineModeLabel: string = ``;
        if(offlineMode){
            offlineModeLabel = `
            <div class="ms-Grid-col ms-sm2">
                <strong class="${styles.warning}">Remote server cannot be reached. Local data is used</strong>
            </div>
            `;
        }

        let template: string = `
            <div class="${styles.row} ${styles.headerFilter}">
                <div class="ms-Grid-col ms-sm2 ${styles.hideWideContent} ${styles.baseBlock} ${styles.block} ${(activeStyle === "All") ? styles.activeBlock : ""}" data-type="All">
                    <strong data-type="All" class="${styles.mainLabel}">All</strong>
                    <span data-type="All"> (${candidates.length})</span>
                </div>
                <div class="ms-Grid-col ms-sm2 ${styles.hideWideContent} ${styles.baseBlock} ${applied.length === 0 ? styles.inactiveBlock : ''} ${styles.block} ${(activeStyle === APPLIED && applied.length > 0) ? styles.activeBlock : ""}" data-type="${APPLIED}">
                    <strong data-type="${APPLIED}" class="${styles.mainLabel}">${APPLIED}</strong>
                    <span data-type="${APPLIED}"> (${applied.length})</span>
                </div>
                <div class="ms-Grid-col ms-sm2 ${styles.hideWideContent} ${styles.baseBlock} ${screening.length === 0 ? styles.inactiveBlock : ''} ${styles.block} ${(activeStyle === SCREENING && screening.length > 0) ? styles.activeBlock : ""}" data-type="${SCREENING}">
                    <strong data-type="${SCREENING}" class="${styles.mainLabel}">${SCREENING}</strong>
                    <span data-type="${SCREENING}"> (${screening.length})</span>
                </div>
                <div class="ms-Grid-col ms-sm2 ${styles.hideWideContent} ${styles.baseBlock} ${interviewing.length === 0 ? styles.inactiveBlock : ''} ${styles.block} ${(activeStyle === INTERVIEWING && interviewing.length > 0) ? styles.activeBlock : ""}" data-type="${INTERVIEWING}">
                    <strong data-type="${INTERVIEWING}" class="${styles.mainLabel}">${INTERVIEWING}</strong>
                    <span data-type="${INTERVIEWING}"> (${interviewing.length})</span>
                </div>
                <div class="ms-Grid-col ms-sm2 ${styles.hideWideContent} ${styles.baseBlock} ${offered.length === 0 ? styles.inactiveBlock : ''} ${styles.block} ${(activeStyle === "Offered" && offered.length > 0) ? styles.activeBlock : ""}" style="border-right: 1px solid rgba(37, 36, 36, 0.25);" data-type="${OFFERED}">
                    <strong data-type="${OFFERED}" class="${styles.mainLabel}">Offer</strong>
                    <span data-type="${OFFERED}"> (${offered.length})</span>
                </div>
                ${offlineModeLabel}
            </div>
        `;

        return template;
    }

    /**
     *  Build position selector template
     * @param positionFilter Current position filter.
     * @param positions Available positions.
     */
    public static BuildPositionSelectorTemplate(positionFilter: string, positions: Position[]): string {
        let allDropdownMenu: string = positions.length === 1 ? `` : `<a class="change-position-btn" data-position="All" href="#all">All</a>`;
        positionFilter = positions.length === 1 ? positions[0].title : positionFilter;
        let positionDropdowns: string = `${allDropdownMenu}`;
        positions.forEach((p) => { positionDropdowns += `<a class="change-position-btn" data-position="${p.title}" href="#${p.title}">${p.title}</a>`; });
        let template = `
            <div class="${styles.row} ${styles.headerFilter}">
                    <div class="ms-Grid-col ms-sm4 ms-textAlignLeft position-dropdown ${styles.dropdown}">
                        <span class="${styles.stageColumn} ${styles.cursorPointer}">Positions (${positionFilter})</span>
                        <i class="ms-Icon ms-Icon--ChevronDown ${styles.icon}" title="Open" aria-hidden="true"></i>
                        <div id="positionsDropdownContent" class="ms-textAlignLeft ${styles.dropdownContent}" style="min-width: 200px;">
                            ${positionDropdowns}
                        </div>
                    </div>
            </div>
        `;

        return template;
    }
}