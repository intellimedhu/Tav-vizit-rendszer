<template>
    <div class="card card-center-profile-editor mb-4" :class="`card-center-profile-editor-${flavor}`">
                <div class="card-header">
                    <h4 class="m-0">
                        {{ paneTitle }}                        
                    </h4>
                </div>

                <div class="card-body">
                    <button type="button"
                            class="btn btn-success mb-3"
                            v-if="paneType == 'pending'"
                            @click="actionButtonClick('invite')">
                        <i class="fa fa-user-plus"></i>
                        Meghívás
                    </button>

                    <div v-if="colleagues.length">
                        <div class="row form-group">
                            <label class="col-12 col-md-4 col-form-label text-md-right" :for="`${paneType}-search`">
                                Keresés
                            </label>
                            <div class="col-12 col-md-8">
                                <input type="search"
                                       :id="`${paneType}-search`"
                                       class="form-control"
                                       placeholder="név, email"
                                       v-model="searchExpression" />
                            </div>
                        </div>

                        <div v-if="colleaguesFiltered.length">
                            <div class="table-responsive">
                                <table class="table table-striped table-hover table-sm table-v-center">
                                    <thead class="thead-light">
                                        <tr>
                                            <th class="border-top-0">Név</th>
                                            <th class="border-top-0">Email</th>
                                            <th class="border-top-0">Munkakör</th>
                                            <th class="border-top-0">Státusz</th>
                                            <th class="border-top-0"></th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <tr v-for="colleague in colleaguesFiltered" v-bind:key="colleague.id">
                                            <td>
                                                <strong>{{ colleague.fullName }}</strong>
                                            </td>
                                            <td>
                                                <a v-if="colleague.email" :href="`mailto:${colleague.email}`">
                                                    {{ colleague.email }}
                                                </a>
                                            </td>
                                            <td>
                                                {{ occupations[colleague.occupation] }}
                                            </td>
                                            <td :title="colleague.statusHistory[0].statusDateUtc | fullDateTime">
                                                <u>{{ colleagueStatuses[colleague.statusHistory[0].status] }}</u>
                                            </td>

                                            <td class="text-right">
                                                <!-- Info button for all cases -->
                                                <div class="d-inline-block">
                                                    <button type="button"
                                                            class="btn btn-sm"
                                                            :class="{'btn-info': colleague.memberRightId, 'btn-outline-secondary': !colleague.memberRightId}"
                                                            :title="colleague.memberRightId ? 'Munkatárs adatlapja' : 'A munkatárs nem tag'"
                                                            @click="viewProfile(colleague)"
                                                            :disabled="!colleague.memberRightId">
                                                        <i class="fas fa-info"></i>
                                                    </button>
                                                </div>

                                                <!-- Active -->
                                                <div v-if="paneType == 'active'" class="d-inline-block">
                                                    <button type="button"
                                                            class="btn btn-sm btn-danger"
                                                            title="Munkatárs törlése"
                                                            @click="actionButtonClick('RemoveActive', colleague.id)">
                                                        <i class="fas fa-trash-alt"></i>
                                                    </button>
                                                </div>

                                                <!-- Pending -->
                                                <div v-if="paneType == 'pending'" class="d-inline-block">
                                                    <button type="button" class="btn btn-sm btn-success"
                                                            v-if="colleague.statusHistory[0].status == 2"
                                                            title="Jelentkezés elfogadása"
                                                            @click="actionButtonClick('AcceptApplication', colleague.id)">
                                                        <i class="fas fa-check"></i>
                                                    </button>
                                                    <button type="button" class="btn btn-sm btn-danger"
                                                            v-if="colleague.statusHistory[0].status == 2"
                                                            title="Jelentkezés elutasítása"
                                                            @click="actionButtonClick('RejectApplication', colleague.id)">
                                                        <i class="fas fa-trash-alt"></i>
                                                    </button>
                                                    <button type="button"
                                                            v-if="colleague.statusHistory[0].status == 3"
                                                            class="btn btn-sm btn-outline-info"
                                                            title="Meghívó újraküldése"
                                                            @click="actionButtonClick('ResendInvitation', colleague.id, paneType)">
                                                        <i class="fas fa-envelope"></i>
                                                    </button>
                                                    <button type="button" class="btn btn-sm btn-danger"
                                                            v-if="colleague.statusHistory[0].status == 3"
                                                            title="Meghívó törlése"
                                                            @click="actionButtonClick('CancelInvitation', colleague.id)">
                                                        <i class="fas fa-trash-alt"></i>
                                                    </button>
                                                </div>

                                                <!-- Removed -->
                                                <div v-if="paneType == 'removed'" class="d-inline-block">
                                                    <button type="button"
                                                            class="btn btn-sm btn-outline-danger"
                                                            title="Meghívó újraküldése"
                                                            @click="actionButtonClick('ResendInvitation', colleague.id, paneType)">
                                                        <i class="fas fa-envelope"></i>
                                                    </button>
                                                </div>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                            
                            <!-- <ul class="pagination justify-content-center">
                                <li class="page-item disabled">
                                    <span class="page-link">Első</span>
                                </li>
                                <li class="page-item">
                                    <a class="page-link" href="#">1</a></li>
                                <li class="page-item active">
                                    <span class="page-link">
                                        2 <span class="sr-only">(current)</span>
                                    </span>
                                </li>
                                <li class="page-item">
                                    <a class="page-link" href="#">3</a>
                                </li>
                                <li class="page-item">
                                    <a class="page-link" href="#">Utolsó</a>
                                </li>
                            </ul> -->
                        </div>

                        <p v-if="!colleaguesFiltered.length" class="text-muted m-0">
                            Nincs a keresési feltételnek megfelelő személy.
                        </p>
                    </div>

                    <p v-if="!colleagues.length" class="text-muted m-0">
                        Nincsenek személyek
                    </p>
                </div>
            </div>
</template>
<script>
import moment from "moment";
import { EventBus } from "../event-bus";

export default {
  data() {
    return {
      searchExpression: null
    };
  },
  methods: {
    actionButtonClick(action, colleagueId, senderPane) {
      EventBus.$emit("colleague-panel-action", {
        action: action,
        id: colleagueId,
        senderPane: senderPane
      });
    },

    viewProfile(colleague) {
      EventBus.$emit("view-colleague-profile", {
        colleague: colleague
      });
    }
  },
  computed: {
    colleagueStatuses() {
      return this.$store.getters.colleagueStatuses;
    },
    occupations() {
      return this.$store.getters.occupations;
    },
    colleaguesFiltered() {
      if (!this.searchExpression) {
        return this.colleagues;
      }

      return this.colleagues.filter(colleague => {
        return Object.keys(colleague)
          .filter(key => ["fullName", "email"].some(y => y == key))
          .some(key => {
            return (
              String(colleague[key])
                .toLowerCase()
                .indexOf(this.searchExpression) > -1
            );
          });
      });
    }
  },
  filters: {
    fullDateTime(date) {
      if (!date) {
        return;
      }

      return moment(date).format("YYYY.MM.DD. HH:mm");
    }
  },
  props: ["paneTitle", "paneType", "colleagues", "flavor"]
};
</script>
