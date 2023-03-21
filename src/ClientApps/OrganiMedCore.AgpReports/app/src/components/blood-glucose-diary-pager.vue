<template>
    <div class="col col-diary-pager mx-auto slide-buttons">
        <div class="input-group input-group-sm">
            <div class="input-group-prepend">
                <button type="button"
                        class="btn btn-sm btn-primary btn-slide btn-slide-back"
                        @click="slideBack"
                        :disabled="!canSlideBack">
                    <i class="icon-chevron-left"></i>
                </button>
            </div>

            <input type="date"
                   class="form-control form-control-sm text-center"
                   required="required"
                   v-model="diarySmDateFormat"
                   :min="diarySmDateMin"
                   :max="diarySmDateMax"
                   @change="diarySmDateChanged"
                   :disabled="!canSlideBack && !canSlideForward" />

            <div class="input-group-append">
                <button type="button"
                        class="btn btn-sm btn-primary btn-slide btn-slide-forward"
                        @click="slideForward"
                        :disabled="!canSlideForward">
                    <i class="icon-chevron-right"></i>
                </button>
            </div>
        </div>
    </div>
</template>

<script>
import moment from "moment";
import { EventBus } from "../services/event-bus";

export default {
  data() {
    return {
      diarySmDateFormat: null
    };
  },
  methods: {
    slideBack() {
      if (this.canSlideBack) {
        this.setDiaryDate(
          moment(this.diarySelectedDay)
            .subtract(1, "days")
            .toDate()
        );
        this.onDiarySmDateChanged();
      }
    },

    slideForward() {
      if (this.canSlideForward) {
        this.setDiaryDate(
          moment(this.diarySelectedDay)
            .add(1, "days")
            .toDate()
        );

        this.onDiarySmDateChanged();
      }
    },

    diarySmDateChanged() {
      let date = new Date(this.diarySmDateFormat);

      let outOfRange = false;
      if (!moment(date).isValid()) {
        date = this.diarySelectedDay;
        outOfRange = true;
      } else if (date > this.selectedMaxDate) {
        date = this.selectedMaxDate;
        outOfRange = true;
      } else if (date < this.selectedMinDate) {
        date = this.selectedMinDate;
        outOfRange = true;
      }

      this.setDiaryDate(date);
      if (outOfRange) {
        this.onDiarySmDateChanged();
      }
    },

    setDiaryDate(date) {
      this.$store.commit("setDiaryDate", date);
    },

    onDiarySmDateChanged() {
      this.$nextTick(() => {
        this.diarySmDateFormat = moment(this.diarySelectedDay).format(
          "YYYY-MM-DD"
        );
      });
    }
  },
  computed: {
    selectedMinDate() {
      return this.$store.state.mainModule.range.selectedMinDate;
    },
    selectedMaxDate() {
      return this.$store.state.mainModule.range.selectedMaxDate;
    },
    diarySelectedDay() {
      return this.$store.state.mainModule.diarySelectedDay;
    },
    diarySmDateMin() {
      return moment(this.selectedMinDate).format("YYYY-MM-DD");
    },
    diarySmDateMax() {
      return moment(this.selectedMaxDate).format("YYYY-MM-DD");
    },
    canSlideBack() {
      return moment(this.diarySelectedDay).isAfter(this.selectedMinDate);
    },
    canSlideForward() {
      return moment(this.diarySelectedDay)
        .endOf("day")
        .isBefore(this.selectedMaxDate);
    }
  },
  mounted() {
    this.onDiarySmDateChanged();

    EventBus.$on("diary-sm-date-changed", this.onDiarySmDateChanged);
  },
  beforeDestroy() {
    EventBus.$off("diary-sm-date-changed", this.onDiarySmDateChanged);
  }
};
</script>
